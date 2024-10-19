namespace IMS2.Models
{
    public class UpdateInvoiceNoModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? ApplicationNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? DocketNo { get; set; }
        public string? Reason { get; set; }
    }
}
