using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IBookedOrdersToSO
    {
        Task<List<BookedOrdersToSOModel>> UpdateBookedOrdersSO(List<BookedOrdersToSOModel> transactions);
    }
}
