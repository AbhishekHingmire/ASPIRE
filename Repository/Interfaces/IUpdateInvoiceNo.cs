using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUpdateInvoiceNo
    {
        Task<List<UpdateInvoiceNoModel>> BulkUpdateInvoiceNo(List<UpdateInvoiceNoModel> transactions);
    }
}
