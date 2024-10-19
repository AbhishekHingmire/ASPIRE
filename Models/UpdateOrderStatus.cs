namespace IMS2.Models
{
    public class UpdateOrderStatusModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? AppNo { get; set; }
        public string? StatusCode { get; set; }
        public string? Remarks { get; set; }
    }
}
