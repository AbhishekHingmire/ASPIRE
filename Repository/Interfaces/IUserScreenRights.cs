using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUserScreenRights
    {
        IEnumerable<UserScreenRight> GetScreen();
        IEnumerable<UserScreen> GetScreenRights();
        IEnumerable<UserDetails> GetUserDetails();
        bool CreateOrEditUserScreenRights(UserScreenRightsModel model);
        List<UserScreenRightsViewModel> GetAllUserScreenRights(int pageNumber, int pageSize);
        bool UpdateScreenRights(UpdateUserScreenRightsModel model);
        void DeleteScreenRights(long id);
    }
}
