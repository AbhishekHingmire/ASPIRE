using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IBranchStockTransferAfterInvoice
    {
        Task<List<BranchStockTransferAfterInvoiceModel>> BranchStockTransfer(List<BranchStockTransferAfterInvoiceModel> transactions);
    }
}
