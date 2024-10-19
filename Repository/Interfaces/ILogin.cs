using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface ILogin
    {
        Users GetUserByUsernameAndPassword(string username, string password);
        Dictionary<string, string> GetUserRightsByScreens(long UserID);
    }
}
