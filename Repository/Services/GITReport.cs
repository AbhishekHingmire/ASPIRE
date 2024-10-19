using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class GITReport : IGITReport
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GITReport> _logger;

        public GITReport(ApplicationDbContext context, ILogger<GITReport> logger)
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

        public async Task<IEnumerable<string>> GetBranchAsync(long partnerID)
        {
            try
            {
                var query = await Task.Run(() =>
                    new[] { "-" }
                        .Union(
                            _context.ERPOrder
                                .Where(o => o.PartnerID == partnerID && !string.IsNullOrEmpty(o.Region))
                                .Select(o => o.Region)
                                .Distinct()
                        )
                        .OrderBy(name => name)
                        .ToList()
                );

                return query;
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

                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching item names.");
                return new List<SelectListItem> { new SelectListItem { Value = "-1", Text = "--Error Occurred--" } };
            }
        }
    }
}
