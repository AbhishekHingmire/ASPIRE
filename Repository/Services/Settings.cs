using DocumentFormat.OpenXml.Spreadsheet;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class Settings : ISettings
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Settings> _logger;

        public Settings(ApplicationDbContext context, ILogger<Settings> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SettingsModel>> GetPartnersAsync( long UserID)
        {
            try
            {
                var partners = await _context.Settings
                    .FromSqlRaw("EXEC usp_GetMFIByUser @UserID", new Microsoft.Data.SqlClient.SqlParameter("@UserID", UserID))
                    .ToListAsync();

                return partners;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<ScreenResult>> GetScreensByUserId(long userId)
        {
            try
            {
                var screenResult = await _context.ScreenResults
                    .FromSqlRaw("EXEC usp_GetScreensByUserID @UserID", new SqlParameter("@UserID", userId))
                    .ToListAsync();

                return screenResult;
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                throw new InvalidOperationException("An error occurred while retrieving screens.", ex);
            }
        }


    }
}
