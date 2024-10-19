using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUpdateTransitFields
    {
        Task<List<UpdateTransitFieldsModel>> UpdateTransitData(List<UpdateTransitFieldsModel> transactions);
    }
}
