using IMS2.Models;

namespace IMS2.Repository.Interfaces
{
    public interface IUploadDummyOrderReassignment
    {
        Task<List<UploadDummyOrderReassignmentModel>> UploadDummyOrder(List<UploadDummyOrderReassignmentModel> transactions);
    }
}
