using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IImportGRN
    {
        Task<List<ImportGRNModel>> ImportGRNs(List<ImportGRNModel> transactions);
    }
}
