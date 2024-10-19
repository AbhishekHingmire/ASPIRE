using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface ISettings
    {
        Task<IEnumerable<SettingsModel>> GetPartnersAsync(long UserID);
        Task<IEnumerable<ScreenResult>> GetScreensByUserId(long userId);
    }
}
