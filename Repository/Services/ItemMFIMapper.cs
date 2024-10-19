using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class ItemMFIMapper : IItemMFIMapper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ItemMFIMapper> _logger;

        public ItemMFIMapper(ApplicationDbContext context, ILogger<ItemMFIMapper> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ItemMFIMapperModel>> GetItemMFIMapperListAsync(long partnerId, int pageNumber, int pageSize)
        {
            try
            {
                var result = await _context.PartnerItems
                    .FromSqlRaw("EXEC dbo.api_Admin_ItemMFI_List @PartnerID = {0}, @PageNumber = {1}, @PageSize = {2}", partnerId, pageNumber, pageSize)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching partner items for PartnerID {PartnerID}", partnerId);
                throw;
            }
        }

        public async Task<IEnumerable<ItemCBO>> GetItemCBOsAsync()
        {
            try
            {
                return await _context.ItemCBOs
                    .FromSqlRaw("SELECT ID, Code + ' (' + Name + ')' AS Name FROM dbo.Item WHERE ID <> -1 ORDER BY Code")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item CBOs");
                throw;
            }
        }


        public bool CreateAndEditItemMFIMapperAsync(ItemMFIData model, long adminUserId, long partnerId)
        {
            try
            {
                var existingItem = _context.PartnerItem
                    .FirstOrDefault(item => item.PartnerID == partnerId && item.ItemID == model.ItemID);

                if (existingItem != null && existingItem.ID != model.DT_RowId)
                {
                    return false;
                }

                _context.Database.ExecuteSqlRaw(
                    "EXEC api_Admin_ItemMFI_Edit @ID, @ItemID, @MRP, @Price, @GSTPercent, @IsLMD, @PartnerID, @AdminUserID",
                    new[]
                    {
                        new SqlParameter("@ID", model.DT_RowId),
                        new SqlParameter("@ItemID", model.ItemID),
                        new SqlParameter("@MRP", model.MRP),
                        new SqlParameter("@Price", model.Price),
                        new SqlParameter("@GSTPercent", model.GSTPercent),
                        new SqlParameter("@IsLMD", model.IsLMD),
                        new SqlParameter("@PartnerID", partnerId),
                        new SqlParameter("@AdminUserID", adminUserId),
                    });

                return true; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating item master: {ex.Message}");
            }
        }


        public void DeleteItem(long dtRowId, long AdminUserID)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete item with ID {dtRowId} by AdminUserID {AdminUserID}.");

                _context.Database.ExecuteSqlRaw(
                   "EXEC api_Admin_ItemMFI_Delete @ID, @AdminUserID",
                   new SqlParameter("@ID", dtRowId),
                   new SqlParameter("@AdminUserID", AdminUserID)
               );

                _logger.LogInformation($"Item with ID {dtRowId} deleted successfully by AdminUserID {AdminUserID}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting Item with ID {dtRowId}: {ex.Message}");
                throw;
            }
        }
    }
}
