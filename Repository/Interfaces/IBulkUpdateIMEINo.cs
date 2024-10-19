using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IBulkUpdateIMEINo
    {
        Task<List<BulkUpdateIMEINoModel>> UpdateIMEINo(List<BulkUpdateIMEINoModel> transactions);
    }
}
