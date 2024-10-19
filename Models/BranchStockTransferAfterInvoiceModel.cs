namespace IMS2.Models
{
    public class BranchStockTransferAfterInvoiceModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? ApplicationNo { get; set; }
        public string? ToBranchCode { get; set; }
    }
}
