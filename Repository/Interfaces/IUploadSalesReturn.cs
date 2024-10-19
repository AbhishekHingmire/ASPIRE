using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUploadSalesReturn
    {
        Task<List<UploadSalesReturnModel>> BulkUploadSalesReturn(List<UploadSalesReturnModel> transactions);
    }
}
