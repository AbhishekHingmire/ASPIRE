namespace IMS2.Models
{
    public class DeleteSalesOrderModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? LoanAppNo { get; set; }
        public string? Remark { get; set; }

    }
}
