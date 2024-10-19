using IMS2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS2.Repository.Interfaces
{
    public interface IBranchComplaintHO
    {
        Task<List<SelectListItem>> GetItemNamesAsync(long partnerId);
        Task<IEnumerable<BranchComplaintHOModel>> GetBranchComplaintHO(string branchCode, long itemId, string regionCode, long branchTypeID, long branchID, long partnerId, int pageNumber, int pageSize);
    }
}
