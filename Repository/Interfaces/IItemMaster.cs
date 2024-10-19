using IMS2.Models;
using IMS2.Repository.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Velzon.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IItemMaster
    {
        Task<ServiceResponse<List<GetItemMasterModel>>> GetItemMasterListAsync(int pageNumber, int pageSize);
        Task<ServiceResponse<List<Category>>> GetCategoriesAsync();
        Task<ServiceResponse<bool>> CreateEditItemMasterAsync(ItemMasterModel model, long AdminUserID);
        Task<ServiceResponse<bool>> DeleteItemAsync(long Id, long AdminUserID);
    }
}
