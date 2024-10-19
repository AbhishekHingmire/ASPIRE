using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IBranch
    {
        Task<List<BranchModel>> GetBranchListAsync(long partnerId, int pageNumber, int pageSize);
        Task<IEnumerable<BranchTypeModel>> GetBranchAsync();
        Task<IEnumerable<ParentBranchModel>> GetParentBranches(long branchTypeID, long partnerID);
        Task<long> CreateUserAndProcessBranchAsync(BranchMasterModel model);
        void DeleteUser(long id);
    }
}
