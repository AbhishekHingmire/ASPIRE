using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class ImportSalesOrder
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public string StoredProc { get; set; }
        public string ParamTitles { get; set; }
        public string ParamColWidth { get; set; }
        public int FrozonCols { get; set; }
        public string ExcelTabName { get; set; }
        public string ExcelPassword { get; set; }
        public string ExportStoredProc { get; set; }
    }

    public class TransactionModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public string? WareHouseCode { get; set; }
        public string? WareHouseName { get; set; }
        public string? OrderNo { get; set; }
        [Key]
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
        public string? AltContactNo { get; set; }
        public string? ShipToAddress { get; set; }
        public string? ShipToGSTNo { get; set; }
        public string? BillFromStateName { get; set; }
        public string? BillFromStateCode { get; set; }
        public string? Reason { get; set; }
    }

    public class FailedTransactionModel
    {
        public string? LoanAppNo { get; set; }
        public string? OrderNo { get; set; }
        public string? Reason { get; set; }
    }
}
