namespace IMS2.Models
{
    public class ChangeInvoiceNoModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? LoanAppNo { get; set; }
        public string? OldInvoiceNo { get; set; }
        public string? NewInvoiceNo { get; set; }
    }
}
