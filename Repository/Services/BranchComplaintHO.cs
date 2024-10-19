using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class BranchComplaintHO : IBranchComplaintHO
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BranchComplaintHO> _logger;

        public BranchComplaintHO(ApplicationDbContext context, ILogger<BranchComplaintHO> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<IEnumerable<BranchComplaintHOModel>> GetBranchComplaintHO(string branchCode, long itemId, string regionCode, long branchTypeID, long branchID, long partnerId, int pageNumber, int pageSize)
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

                var branchStockData = await _context.BranchComplaintHO
                    .FromSqlRaw(
                        "EXEC [dbo].[usp_GetBranchComplaintHOList_New] @BranchCode, @ItemID, @PartnerID, @LoggedInBranchTypeID, @LoggedInBranchID, @RegionCode, @PageNumber, @PageSize",
                        new SqlParameter("@BranchCode", branchCode),
                        new SqlParameter("@ItemID", itemId),
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

                throw;
            }
        }

    }
}
