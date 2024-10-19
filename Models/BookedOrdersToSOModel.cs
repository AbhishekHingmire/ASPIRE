namespace IMS2.Models
{
    public class BookedOrdersToSOModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public string? WareHouseCode { get; set; }
        public string? WareHouseName { get; set; }
        public string? BookedLoanAppNo { get; set; }
        public string? SKU { get; set; }
        public string? ProductName { get; set; }
        public int Qty { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? BillToAddressStateCode { get; set; }
        public string? GSTNo { get; set; }
        public int DP { get; set; }
        public string? IMEI_No { get; set; }
        public string? DPStatus { get; set; }
        public string? AltContactNo { get; set; }
    }
}
