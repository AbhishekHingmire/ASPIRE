using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IItemMFIMapper
    {
        Task<List<ItemMFIMapperModel>> GetItemMFIMapperListAsync(long partnerId, int pageNumber, int pageSize);
        Task<IEnumerable<ItemCBO>> GetItemCBOsAsync();
        bool CreateAndEditItemMFIMapperAsync(ItemMFIData model, long adminUserId, long partnerId);
        void DeleteItem(long dtRowId, long AdminUserID);
    }
}
