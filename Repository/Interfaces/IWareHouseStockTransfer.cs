using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IWareHouseStockTransfer
    {
        Task<List<WareHouseModel>> GetWareHouseList();
        Task<List<ItemMasterModel>> GetItemList();
        Task<long> GetStockAsync(long wareHouseID, long itemID);
        Task<bool> DoWareHouseStockTransfer(WareHouseStockTransferModel model);
    }
}
