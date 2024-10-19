using DocumentFormat.OpenXml.Wordprocessing;
using IMS2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS2.Repository.Interfaces
{
    public interface IBranchStock
    {
        Task<IEnumerable<BranchDetails>> GetBranchAsync(long partnerID, long branchTypeId, string roleClaim, string name, long userId, long branchID);
        Task<IEnumerable<BranchDetails>> GetBranchCodesByTypeAsync(long partnerID, long regionId, string roleClaim, string name, long branchTypeID, long branchID);
        Task<IEnumerable<BranchStockModel>> GetAllTableData(string branchCode, long itemId, string datestamp, string regionCode, long branchTypeID, long branchID, long partnerId, int pageNumber, int pageSize);
        Task<List<SelectListItem>> GetItemNamesAsync(long partnerId);
    }
}
