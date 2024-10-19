using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IImportSpecialTransaction
    {
        Task<List<FailedTransactionModel>> AddTransactionsAsync(List<ImportSpecialTransactionModel> transactions);
    }
}
