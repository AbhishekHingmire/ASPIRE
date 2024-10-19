using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class DisbursementReport : IDisbursementReport
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DisbursementReport> _logger;

        public DisbursementReport(ApplicationDbContext context, ILogger<DisbursementReport> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DisbursementReportModel>> GetBranchNameAndRegionName(string branchCode, string regionCode)
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

                var result = new List<DisbursementReportModel>
                {
                    new DisbursementReportModel{ RegionCode = regionCode,BranchCode = branchCode }
                };

                return result;
            }
            catch (Exception ex)
            {
                // Optionally log the exception
                // _logger.LogError(ex, "An error occurred while retrieving branch stock data.");
                throw;
            }
        }

    }
}
