namespace IMS2.Models
{
    public class ImportGRNModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? PORef {  get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvoiceDate { get; set; }
        public string? Month { get; set; }
        public string? Supplier {  get; set; }
        public string? City { get; set; }
        public string? WareHouseCode { get; set; }
        public string? WareHouseName { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public int Qty { get; set; }
        public string? DeliveryDate { get; set; }
        public string? IMEI_No { get; set; }
        public string? InvoiceFile { get; set; }
        public int InvQty { get; set; }
        public string? DiffRemarks { get; set; }
    }
}
