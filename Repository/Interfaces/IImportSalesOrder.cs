using IMS2.Models;
using System.Data;

namespace IMS2.Repository.Interfaces
{
    public interface IImportSalesOrder
    {
        Task<List<FailedTransactionModel>> AddTransactionsAsync(List<TransactionModel> transactions);
        List<DataTable> ExecSQL(string CommandText);
        string GetProcedureSignatureInParts(string procedure);
        string GetProcedureSignature(string procedure);
        DataTable GetProceduresParameters(string procedure, bool ignoreSpecialCols);
        DataTable ReadAsDataTable(Stream inputStream, int tableRowStartIndex);
        string FormatDatepickerToDatestamp(string str);
        int ExecNonQuery(string commandText);
        bool IsSQLInjection(string DataSQL);
    }
}
