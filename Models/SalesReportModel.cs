namespace IMS2.Models
{
    public class SalesReportModel
    {
        public string PartyName { get; set; }
        public string GSTNo { get; set; }
        public string BillFrom { get; set; }
        public string ShipFrom { get; set; }
        public string OrderNo { get; set; }
        public string LoanApplicationNo { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public int Qty { get; set; }
        public decimal MRP { get; set; }
        public decimal BasicRate { get; set; }
        public decimal GSTPercent { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal SaleRate { get; set; }
        public decimal TotalAmount { get; set; }
        public long StateCode { get; set; }
        public string StateName { get; set; }
        public string BillToAddress { get; set; }
        public string ShipToAddress { get; set; }
        public string ContactNo { get; set; }
        public string SpouseName { get; set; }
        public string OrderDate { get; set; }
        public string IMEINo { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string MFI { get; set; }
        public string DeliveryDate { get; set; }
        public string DocumentUploadDate { get; set; }
        public string AlternateContactNo { get; set; }
        public string CODorPrepaid { get; set; }
        public string AgainstOriginalInvoiceNo { get; set; }
        public string DeliveryBoyRemarks { get; set; }
        public string DeliveryBoyDate { get; set; }
        public string DamageType { get; set; }
        public string PODAttached { get; set; }
        public string PODAttachedDate { get; set; }
        public string PODImage { get; set; }
        public string DeliveryBoyLocation { get; set; }
        public string AddressLocation { get; set; }
        public string DisbursmentStatus { get; set; }
        public string GrNo { get; set; }
        public string EDD { get; set; }
        public string TransporterName { get; set; }
        public string Mode { get; set; }
        public decimal GrossWeight { get; set; }
        public string DisbursmentDate { get; set; }
        public string PODImage2 { get; set; }
        public string PODImage3 { get; set; }
        public string PODImage4 { get; set; }

    }

    public class ERPOrders
    {
        public int Id { get; set; } 
        public long PartnerID { get; set; }
        public string BillToAddressStateCode { get; set; }
        public string Region { get; set; }
    }

    public class Provinces
    {
        public int Id { get; set; } 
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
