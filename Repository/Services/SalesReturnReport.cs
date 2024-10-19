using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class SalesReturnReport : ISalesReturnReport
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SalesReturnReport> _logger;

        public SalesReturnReport(ApplicationDbContext context, ILogger<SalesReturnReport> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SalesReturnReportModel>> GetSalesReturnReport(long partnerId, int pageNumber, int pageSize)
        {
            try
            {
                var result = await _context.SalesReturnReport
                    .FromSqlRaw("EXEC dbo.usp_GetSalesReturnList @PartnerID = {0}, @PageNumber = {1}, @PageSize = {2}", partnerId, pageNumber, pageSize)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Sales return report for PartnerID {PartnerID}", partnerId);
                throw;
            }
        }
    }
}
