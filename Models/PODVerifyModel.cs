namespace IMS2.Models
{
    public class PODVerifyModel
    {
        public string POD_IMG { get; set; }
        public string IDProofFrontIMG { get; set; }
        public string IDProofBackIMG { get; set; }
        public string InvoiceIMG { get; set; }
        public string DeliveryBoyRemarks { get; set; }
        public string DeliveryDate { get; set; }
        public long ERPOrderStatus { get; set; }
    }

    public class FileUrl
    {
        public string Url { get; set; }
    }
}
