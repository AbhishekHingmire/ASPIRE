using IMS2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS2.Repository.Interfaces
{
    public interface IGITReport
    {
        Task<List<string>> GetProvinceNames(string roleClaim, long partnerId, string name = null);
        Task<IEnumerable<string>> GetBranchAsync(long partnerID);
        Task<List<SelectListItem>> GetItemNamesAsync(long partnerId);
    }
}
