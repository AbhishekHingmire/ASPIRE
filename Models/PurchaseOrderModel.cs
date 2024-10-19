using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class PurchaseOrderModel
    {
        [Key]
        public long DT_RowId { get; set; }
        public string? PO_No { get; set; }
        public string? PORef { get; set; }
        public string? OrderDate { get; set; }
        public long POTypeID { get; set; }
        public string? POType { get; set; }
        public long SupplierID { get; set; }
        public string? Supplier { get; set; }
        public string? Receiver_GST { get; set; }
        public string? BillTo { get; set; }
        public string? ShipTo { get; set; }
        public long ItemID { get; set; }
        public string? Item { get; set; }
        public string? Description { get; set; }
        public int Qty { get; set; }
        public string? Unit { get; set; }
        public decimal PriceExcludingGST { get; set; }
        public decimal GSTPercent { get; set; }
        public decimal PriceIncludingGST { get; set; }
        public decimal TotalAmount { get; set; }
        public int AvailableQty { get; set; }
        public string? Partner { get; set; }
        public int ReceivedQty { get; set; }
        public string? PaymentTerms { get; set; }
        public long POCompanyID { get; set; }
        public string? PO_Company { get; set; }
    }


    public class PurchaseOrders
    {
        [Key]
        public long DT_RowId { get; set; }
        public string PONo { get; set; }
        public string PORef { get; set; }
        public string OrderDate { get; set; } 
        public long POTypeID { get; set; }
        public long SupplierID { get; set; }
        public string ReceiverGST { get; set; }
        public string BillTo { get; set; }
        public string ShipTo { get; set; }
        public long ItemID { get; set; }
        public string Description { get; set; }
        public int Qty { get; set; }
        public string Unit { get; set; }
        public decimal PriceExcludingGST { get; set; }
        public decimal GSTPercent { get; set; }
        public decimal PriceIncludingGST { get; set; }
        public decimal TotalAmount { get; set; }
        public long PartnerID { get; set; }
        public string PaymentTerms { get; set; }
        public long AdminUserID { get; set; }
        public long POCompanyID { get; set; }
    }

    public class POTypes
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    public class POCompanys
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    public class Suppliers
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    public class ItemViewModel
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }
    public class DataTableList
    {
        public List<PurchaseOrderModel> Orders { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; } 
    }
}
