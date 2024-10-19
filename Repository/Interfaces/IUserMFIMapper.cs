using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUserMFIMapper
    {
        List<UserMFIMapperModel> GetAllAdminUserMFIList(int pageNumber, int pageSize);
        Task<List<MFIList>> GetPartnersAsync();
        IEnumerable<UserDetails> GetUserDetails();
        bool CreateOrEditUserUserMFIScreen(UserMFIMapperCreateAndEditModel model);
        void DeleteUserMFIMapper(long id);
    }
}
