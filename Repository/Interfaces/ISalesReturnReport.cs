using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface ISalesReturnReport
    {
        Task<List<SalesReturnReportModel>> GetSalesReturnReport(long partnerId, int pageNumber, int pageSize);
    }
}
