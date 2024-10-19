namespace IMS2.Models
{
    public class UploadDummyOrderReassignmentModel
    {
        public long AdminUserID { get; set; }
        public string? OldLoanAppNo { get; set; }
        public string? NewLoanAppNo { get;set; }
        public string? CustomerName { get; set; }
        public string? Reason { get; set; }
    }
}
