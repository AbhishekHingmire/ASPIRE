namespace IMS2.Models
{
    public class UploadSalesReturnModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? AppNo { get; set; }
        public string? CreditNoteInvoiceNo { get; set; }
        public string? CustomRemark { get; set; }
    }
}
