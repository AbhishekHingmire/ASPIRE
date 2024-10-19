using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class SalesReturnReportModel
    {
        [Key]
        public string? OrderNo { get; set; }
        public string? LoanAppNo { get; set; }
        public string? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public string? SKU { get; set; }
        public string? ProductName { get; set; }
        public int Qty { get; set; }
        public decimal MRP { get; set; }
        public decimal BasicRate { get; set; }
        public decimal GSTPercent { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal SaleRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? BranchID { get; set; }
        public string? BranchName { get; set; }
        public string? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? AgainstOriginalInvoiceNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvoiceDate { get; set; }
    }
}
