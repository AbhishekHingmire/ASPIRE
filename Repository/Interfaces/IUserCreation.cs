using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUserCreation
    {
        void CreateAndUpdateUser(UserCreationModel userModel);
        IEnumerable<RoleModel> GetRoles();
        List<UserCreationGetDetailsModel> GetAllUserCreation(int pageNumber, int pageSize);
        void DeleteUser(long id);
    }
}
