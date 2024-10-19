namespace IMS2.Models
{
    public class ImportSpecialTransactionModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public string? WareHouseCode { get; set; }
        public string? WareHouseName { get; set; }
        public string? OrderNo { get; set; }
        public string? LoanAppNo { get; set; }
        public string? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public string? SKU { get; set; }
        public string? ProductName { get; set; }
        public int Qty { get; set; }
        public string? Region { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? BillToAddressStateCode { get; set; }
        public string? BillToAddress { get; set; }
        public string? ContactNo { get; set; }
        public string? SpouseName { get; set; }
        public string? GSTNo { get; set; }
        public int DP { get; set; }
        public string? IMEI_No { get; set; }
        public string? DPStatus { get; set; }
        public decimal MRP { get; set; }
        public decimal Price { get; set; }
        public decimal GSTPercent { get; set; }
        public string? ShipToAddress { get; set; }
        public string? ShipToGSTNo { get; set; }
        public string? Reason { get; set; }
    }
}
