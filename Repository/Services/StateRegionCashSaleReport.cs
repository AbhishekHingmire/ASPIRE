using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class StateRegionCashSaleReport : IStateRegionCashSaleReport
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StateRegionCashSaleReport> _logger;

        public StateRegionCashSaleReport(ApplicationDbContext context, ILogger<StateRegionCashSaleReport> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<StateRegionCashSaleReportModel>> GetAllTableData(long partnerId, long branchTypeID, long branchID, int pageNumber, int pageSize)
        {
            try
            {
                var branchStockData = await _context.StateRegionCashSale
                    .FromSqlRaw(
                        "EXEC [dbo].[api_Admin_StateRegionCashSale_List_New] @PartnerID, @LoggedInBranchTypeID, @LoggedInBranchID, @PageNumber, @PageSize",
                        new SqlParameter("@PartnerID", partnerId),
                        new SqlParameter("@LoggedInBranchTypeID", branchTypeID),
                        new SqlParameter("@LoggedInBranchID", branchID),
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
    }
}
