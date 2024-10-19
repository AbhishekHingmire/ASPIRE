using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Velzon.Models;

namespace IMS2.Repository.Services
{
    public class ItemMaster : IItemMaster
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ItemMaster> _logger;

        public ItemMaster(ApplicationDbContext context, ILogger<ItemMaster> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResponse<List<GetItemMasterModel>>> GetItemMasterListAsync(int pageNumber, int pageSize)
        {
            var response = new ServiceResponse<List<GetItemMasterModel>>();
            try
            {
                response.Data = await _context.GetItemMasterModels
                    .FromSqlRaw("EXECUTE dbo.api_Admin_Item_List @PageNumber = {0}, @PageSize = {1}",  pageNumber, pageSize)
                    .ToListAsync();
                response.Message = "Item master list fetched successfully.";
                response.Code = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item master list");
                response.Success = false;
                response.Code = 400;
                response.Message = $"Error fetching item master list: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<Category>>> GetCategoriesAsync()
        {
            var response = new ServiceResponse<List<Category>>();
            try
            {
                response.Data = await _context.Category
                    .OrderByDescending(c => c.ID)
                    .ToListAsync();
                response.Message = "Categories fetched successfully.";
                response.Code = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories");
                response.Success = false;
                response.Code = 400;
                response.Message = $"Error fetching categories: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> CreateEditItemMasterAsync(ItemMasterModel model, long AdminUserID)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                if(model.ID == -1)
                {
                    var existingCode = await _context.Item.FirstOrDefaultAsync(u => u.Code == model.Code);
                    if (existingCode != null)
                    {
                        response.Success = false;
                        response.Code = 400;
                        response.Message = "Code already exists.";
                        return response;
                    }
                }

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC api_Admin_Item_Edit @ID, @CategoryID, @Code, @Name, @ShortDesc, @HSNCode, @AdminUserID",
                    new[]
                    {
                        new SqlParameter("@ID", model.ID),
                        new SqlParameter("@CategoryID", model.CategoryID),
                        new SqlParameter("@Code", model.Code),
                        new SqlParameter("@Name", model.Name),
                        new SqlParameter("@ShortDesc", model.ShortDesc ?? ""),
                        new SqlParameter("@HSNCode", model.HSNCode ?? ""),
                        new SqlParameter("@AdminUserID", AdminUserID)
                    });
                response.Data = true;
                response.Message = "Item master created/edited successfully.";
                response.Code = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/editing item master");
                response.Success = false;
                response.Code = 400;
                response.Message = $"Error creating/editing item master: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteItemAsync(long Id, long AdminUserID)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                _logger.LogInformation($"Attempting to delete item with ID {Id} by AdminUserID {AdminUserID}.");

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC api_Admin_Item_Delete @ID, @AdminUserID",
                    new SqlParameter("@ID", Id),
                    new SqlParameter("@AdminUserID", AdminUserID)
                );

                response.Data = true;
                response.Message = $"Item with ID {Id} deleted successfully by AdminUserID {AdminUserID}.";
                response.Code = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting Item with ID {Id}");
                response.Success = false;
                response.Code = 400;
                response.Message = $"Error deleting item with ID {Id}: {ex.Message}";
            }
            return response;
        }
    }
}
