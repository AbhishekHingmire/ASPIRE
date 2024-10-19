using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IChangeInvoiceNo
    {
        Task<List<ChangeInvoiceNoModel>> ChangeInvoice(List<ChangeInvoiceNoModel> transactions);
    }
}
