using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class PODVerify : IPODVerify
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PODVerify> _logger;

        public PODVerify(ApplicationDbContext context, ILogger<PODVerify> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<PODVerifyModel>> GetOrderDetailsForPODVerifyAsync(long partnerId, string applicationNo)
        {
            try
            {
                var partnerIdParam = new SqlParameter("@PartnerID", partnerId);
                var applicationNoParam = new SqlParameter("@ApplicationNo", applicationNo);

                var podDetails = await _context.PODDetails
                                               .FromSqlRaw("EXEC [dbo].[api_Admin_GetOrderDetailsForPODVerify] @PartnerID, @ApplicationNo", partnerIdParam, applicationNoParam)
                                               .ToListAsync();

                return podDetails;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<FileUrl>> GetFilesURLsByApplicationNoAsync(long partnerId, string applicationNo)
        {
            try
            {
                var partnerIdParam = new SqlParameter("@PartnerID", partnerId);
                var applicationNoParam = new SqlParameter("@ApplicationNo", applicationNo);

                var fileUrls = await _context.Set<FileUrl>()
                    .FromSqlRaw("EXEC [dbo].[api_Admin_GetFilesURLByApplicationNo] @PartnerID, @ApplicationNo", partnerIdParam, applicationNoParam)
                    .ToListAsync();

                return fileUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching file URLs");
                throw;
            }
        }

        public async Task MarkOrderDeliveredAsync(string applicationNo)
        {
            try
            {
                var applicationNoParam = new SqlParameter("@ApplicationNo", applicationNo);

                await _context.Database.ExecuteSqlRawAsync("EXEC [dbo].[api_Admin_MarkOrderDelivered] @ApplicationNo", applicationNoParam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while marking order as delivered");
                throw;
            }
        }

    }
}
