using IMS2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS2.Repository.Interfaces
{
    public interface IDisbursementReport
    {
        Task<IEnumerable<DisbursementReportModel>> GetBranchNameAndRegionName(string branchCode, string regionCode);
    }
}
