using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUpdateOrderStatus
    {
        Task<List<FailedTransactionModel>> AddOrderStatusAsync(List<UpdateOrderStatusModel> transactions);
    }
}
