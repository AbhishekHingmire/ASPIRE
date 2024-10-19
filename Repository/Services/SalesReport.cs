using IMS2.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMS2.Models;

namespace IMS2.Repository.Services
{
    public class SalesReport : ISalesReport
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesReport> _logger;

        public SalesReport(ApplicationDbContext context, ILogger<SalesReport> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<string>> GetProvinceNames(string roleClaim, long partnerId, string name = null)
        {
            try
            {
                IQueryable<string> query;

                if (roleClaim != "Warehouse")
                {
                    query = _context.ERPOrder
                        .Where(e => e.PartnerID == partnerId)
                        .Join(
                            _context.Province,
                            e => e.BillToAddressStateCode.Trim(),
                            p => p.Code.Trim(),
                            (e, p) => p.Name)
                        .Distinct()
                        .Union(new[] { "-" }.AsQueryable())
                        .OrderBy(n => n);
                }
                else
                {
                    var ordersWithProvinces = _context.ERPOrder
                        .Where(e => e.PartnerID == partnerId)
                        .Join(
                            _context.Province,
                            e => e.BillToAddressStateCode.Trim(),
                            p => p.Code.Trim(),
                            (e, p) => new { e, p });

                    var ordersWithProvincesAndWarehouses = ordersWithProvinces
                        .Join(
                            _context.WareHouse,
                            ep => ep.p.Code,
                            w => w.Code,
                            (ep, w) => new { ep.p, w });

                    var finalQuery = ordersWithProvincesAndWarehouses
                        .Join(
                            _context.User,
                            epw => epw.w.UserID,
                            u => u.ID,
                            (epw, u) => new { epw.p, u })
                        .Where(epwu => epwu.u.Username == name)
                        .Select(epwu => epwu.p.Name)
                        .Distinct()
                        .OrderBy(n => n);

                    query = finalQuery;
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching province names.");
                return new List<string>();
            }
        }

        public async Task<IEnumerable<BranchDetails>> GetBranchAsync(long partnerID, long branchTypeId, string roleClaim, string name)
        {
            try
            {
                IQueryable<BranchDetails> query = _context.Branch
                    .Where(b => b.ID == -1)
                    .Select(b => new BranchDetails { ID = b.ID, Name = b.Name });

                if (roleClaim == "Branch")
                {
                    if (branchTypeId == -103) // Branch
                    {
                        query = from b in _context.Branch
                                join u in _context.User on b.ID equals u.ID
                                join bh in _context.BranchHierarchy on b.ID equals bh.SubBranchID
                                join br in _context.Branch on bh.BranchID equals br.ID
                                where u.Username == name
                                select new BranchDetails { ID = br.ID, Name = br.Name };
                    }
                    else if (branchTypeId == -99) // HO
                    {
                        query = (from b in _context.Branch
                                 where b.ID != -1 && b.BranchTypeID == -101 && b.PartnerID == partnerID
                                 select new BranchDetails { ID = b.ID, Name = b.Name })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Name = "--All--" }
                            );
                    }
                    else if (branchTypeId == -100) // State
                    {
                        query = (from bh in _context.BranchHierarchy
                                 join b in _context.Branch on bh.SubBranchID equals b.ID
                                 where bh.BranchID == partnerID
                                 select new BranchDetails { ID = b.ID, Name = b.Name })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Name = "--All--" }
                            );
                    }
                    else if (branchTypeId == -101) // Region
                    {
                        query = (from b in _context.Branch
                                 where b.ID == partnerID
                                 select new BranchDetails { ID = b.ID, Name = b.Name })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Name = "--All--" }
                            );
                    }
                    else
                    {
                        query = (from b in _context.Branch
                                 where b.ID != -1 && b.BranchTypeID == -101 && b.PartnerID == partnerID
                                 select new BranchDetails { ID = b.ID, Name = b.Name })
                            .Union(
                                from b in _context.Branch
                                where b.ID == -999
                                select new BranchDetails { ID = b.ID, Name = "--All--" }
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
