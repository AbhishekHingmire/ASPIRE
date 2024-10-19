using IMS2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS2.Repository.Interfaces
{
    public interface ISalesReport
    {
        Task<List<string>> GetProvinceNames(string roleClaim, long partnerId, string name = null);
        Task<List<SelectListItem>> GetItemNamesAsync(long partnerId);
        Task<IEnumerable<BranchDetails>> GetBranchAsync(long partnerID, long branchTypeId, string roleClaim, string name);
    }
}
