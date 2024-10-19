namespace Velzon.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
        public bool Success { get; set; }

        public ServiceResponse()
        {
            Success = true;
        }
    }
}
