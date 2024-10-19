using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IStateRegionCashSaleReport
    {
        Task<IEnumerable<StateRegionCashSaleReportModel>> GetAllTableData(long partnerId, long branchTypeID, long branchID, int pageNumber, int pageSize);
    }
}
