using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;

namespace IMS2.Repository.Services
{
    public class BranchStock : IBranchStock
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BranchStock> _logger;

        public BranchStock(ApplicationDbContext context, ILogger<BranchStock> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<BranchDetails>> GetBranchAsync(long partnerID, long branchTypeId, string roleClaim, string name, long userId, long branchID)
        {
            try
            {
                IQueryable<BranchDetails> query = _context.Branch
                    .Where(b => b.ID == -1) 
                    .Select(b => new BranchDetails { ID = b.ID, Code = b.Code });

                if (roleClaim == "Branch")
                {
                    if (branchTypeId == -103) // Branch
                    {
                        query = from b in _context.Branch
                                join u in _context.User on b.ID equals u.ID
                                join bh in _context.BranchHierarchy on b.ID equals bh.SubBranchID
                                join br in _context.Branch on bh.BranchID equals br.ID
                                where u.Username == name
                                select new BranchDetails { ID = br.ID, Code = br.Code };
                    }
                    else if (branchTypeId == -99) // HO
                    {
                        query = (from b in _context.Branch
                                 where b.ID != -1 && b.BranchTypeID == -101 && b.PartnerID == partnerID
                                 select new BranchDetails { ID = b.ID, Code = b.Code })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Code = "--All--" }
                            );
                    }
                    else if (branchTypeId == -100) // State
                    {
                        query = (from bh in _context.BranchHierarchy
                                 join b in _context.Branch on bh.SubBranchID equals b.ID
                                 where bh.BranchID == userId
                                 select new BranchDetails { ID = b.ID, Code = b.Code })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Code = "--All--" }
                            );
                    }
                    else if (branchTypeId == -101) // Region
                    {
                        query = (from b in _context.Branch
                                 where b.ID == branchID
                                 select new BranchDetails { ID = b.ID, Code = b.Code })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Code = "--All--" }
                            );
                    }
                    else
                    {
                        query = (from b in _context.Branch
                                 where b.ID != -1 && b.BranchTypeID == -101 && b.PartnerID == partnerID
                                 select new BranchDetails { ID = b.ID, Code = b.Code })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Code = "--All--" }
                            );
                    }
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching branches: {ex.Message}");
                throw;
            }
        }


        public async Task<IEnumerable<BranchDetails>> GetBranchCodesByTypeAsync(long partnerID, long regionId, string roleClaim, string name, long branchTypeID, long branchID)
        {
            try
            {
                IQueryable<BranchDetails> query;

                if (roleClaim == "Branch" && branchTypeID == -103)
                {
                    query = from b in _context.Branch
                            join u in _context.User on b.ID equals u.ID
                            where u.Username == name && b.BranchTypeID == -103
                            select new BranchDetails { ID = b.ID, Code = b.Code };
                }
                else if (roleClaim == "Branch" && branchTypeID == -100 && regionId == -999) 
                {
                    query = (
                        from b in _context.Branch
                        join bhRegion in _context.BranchHierarchy on b.ID equals bhRegion.SubBranchID
                        join bRegion in _context.Branch on bhRegion.BranchID equals bRegion.ID
                        join bhState in _context.BranchHierarchy on bRegion.ID equals bhState.SubBranchID
                        where bhState.BranchID == branchID && b.BranchTypeID == -103 && b.PartnerID == partnerID
                        select new BranchDetails { ID = b.ID, Code = b.Code }
                    )
                    .Union(
                        from b in _context.Branch
                        where b.ID == -999
                        select new BranchDetails { ID = b.ID, Code = "--All--" }
                    )
                    .OrderBy(b => b.Code);
                }
                else
                {
                   
                }

                query = (
                       from b in _context.Branch
                       join bh in _context.BranchHierarchy on b.ID equals bh.SubBranchID
                       where bh.BranchID == (regionId == -999 ? bh.BranchID : regionId) && b.BranchTypeID == -103 && b.PartnerID == partnerID
                       select new BranchDetails { ID = b.ID, Code = b.Code }
                   )
                   .Union(
                       from b in _context.Branch
                       where b.ID == -999
                       select new BranchDetails { ID = b.ID, Code = "--All--" }
                   )
                   .OrderBy(b => b.Code);

                var result = await query.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching branch codes: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<BranchStockModel>> GetAllTableData(string branchCode, long itemId, string datestamp, string regionCode, long branchTypeID, long branchID, long partnerId, int pageNumber, int pageSize)
        {
            try
            {
                if (!string.IsNullOrEmpty(regionCode) && regionCode != "--All--")
                {
                    long RegionCode = Convert.ToInt64(regionCode);

                    var regionName = _context.Branch
                        .Where(branch => branch.ID == RegionCode)
                        .Select(branch => branch.Code)
                        .FirstOrDefault();

                    regionCode = Convert.ToString(regionName);
                }

                if (!string.IsNullOrEmpty(branchCode) && branchCode != "--All--")
                {
                    long BranchCode = Convert.ToInt64(branchCode);

                    var branchCodes = _context.Branch
                        .Where(branch => branch.ID == BranchCode)
                        .Select(branch => branch.Code)
                        .FirstOrDefault();

                    branchCode = Convert.ToString(branchCodes);
                }

                var branchStockData = await _context.BranchStockModels
                    .FromSqlRaw(
                        "EXEC [dbo].[usp_GetBranchStock_New] @BranchCode, @ItemID, @Datestamp, @PartnerID, @LoggedInBranchTypeID, @LoggedInBranchID, @RegionCode, @PageNumber, @PageSize",
                        new SqlParameter("@BranchCode", branchCode),
                        new SqlParameter("@ItemID", itemId),
                        new SqlParameter("@Datestamp", datestamp),
                        new SqlParameter("@PartnerID", partnerId),
                        new SqlParameter("@LoggedInBranchTypeID", branchTypeID),
                        new SqlParameter("@LoggedInBranchID", branchID),
                        new SqlParameter("@RegionCode", regionCode),
                        new SqlParameter("@PageNumber", pageNumber),
                        new SqlParameter("@PageSize", pageSize)
                    )
                    .ToListAsync();

                return branchStockData;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                // _logger.LogError(ex, "An error occurred while retrieving branch stock data.");
                throw;
            }
        }

        public async Task<List<SelectListItem>> GetItemNamesAsync(long partnerId)
        {
            try
            {
                var query = _context.Item
                    .Join(_context.PartnerItem,
                          i => i.ID,
                          pit => pit.ItemID,
                          (i, pit) => new { i, pit })
                    .Where(x => x.pit.PartnerID == partnerId)
                    .OrderBy(x => x.i.ID)
                    .Select(x => new SelectListItem
                    {
                        Value = x.i.ID.ToString(),
                        Text = x.i.Name
                    });

                var items = await query.ToListAsync();

                var defaultItem = new SelectListItem { Value = "-1", Text = "--Please Select--" };
                return new List<SelectListItem> { defaultItem }.Concat(items).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching item names.");
                return new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "--Error Occurred--" } };
            }
        }
    }
}
