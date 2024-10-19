using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class PrintBranchSaleInvoiceModel
    {
    }

    public class ItemListByOrderNo
    {
        public string ItemName { get; set; }
        public string ItemHSNCode { get; set; }
        public decimal ItemMRP { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal GSTPercent { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal ItemRate { get; set; }
        public int ItemQty { get; set; }
        public decimal ItemAmount { get; set; }
        public int DPAmt { get; set; }
    }

    public class InvoiceByOrderNo
    {
        [Key]
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string OrderNo { get; set; }
        public string BillToAddress { get; set; }
        public string ShipToAddress { get; set; }
        public string StateName { get; set; }
        public int TotalQty { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalAmount { get; set; }
        public string AmountInWords { get; set; }
        public string OrderDate { get; set; }
        public string BillFromAddress { get; set; }
        public string BillFromGSTNo { get; set; }
        public string Channel { get; set; }
        public string ClientID { get; set; }
        public string LoanAppNo { get; set; }
        public string StateCode { get; set; }
        public string IMEINo { get; set; }
        public string BillToGSTNo { get; set; }
        public string HypoText { get; set; }
        public string DPStatusText { get; set; }
        public List<ItemListByOrderNo> ItemDetails { get; set; }
        public string OriginalInvoiceNo { get; set; }
        public string OriginalInvoiceDate { get; set; }
        public string Title { get; set; }
        public string QRCode { get; set; }
        public string ServiceDetails { get; set; }
        public string DocketNo { get; set; }
        public string Items { get; set; }
    }
}
