using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IPurchaseOrder
    {
        Task<List<PurchaseOrderModel>> GetPurchaseOrderListAsync(long partnerId, int pageNumber, int pageSize);
        Task<List<POTypes>> GetPOTypeList();
        Task<List<POCompanys>> GetPOCompanyList();
        Task<List<Suppliers>> GetSupplierList();
        void CreateOrEditPO(PurchaseOrders model);
        Task<List<ItemViewModel>> GetItemsForPartnerAsync(long partnerId);
        void DeletePo(long id, long AdminUserID);
    }
}
