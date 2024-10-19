using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IDeleteSalesOrder
    {
        Task<List<DeleteSalesOrderModel>> BulkDeleteSalesOrder(List<DeleteSalesOrderModel> transactions);
    }
}
