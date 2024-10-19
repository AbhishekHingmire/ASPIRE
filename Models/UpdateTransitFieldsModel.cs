namespace IMS2.Models
{
    public class UpdateTransitFieldsModel
    {
        public long AdminUserID { get; set; }
        public long PartnerID { get; set; }
        public string? ApplicationNo { get; set; }
        public string? GR_No { get; set; }
        public string? EDD {  get; set; }
        public string? TransporterName { get; set; }
        public string? Mode { get; set; }
        public string? GrossWeight { get; set; }
    }
}
