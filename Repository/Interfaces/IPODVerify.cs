using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IPODVerify
    {
        Task<List<PODVerifyModel>> GetOrderDetailsForPODVerifyAsync(long partnerId, string applicationNo);
        Task<List<FileUrl>> GetFilesURLsByApplicationNoAsync(long partnerId, string applicationNo);
        Task MarkOrderDeliveredAsync(string applicationNo);
    }
}
