using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class UserMFIMapperModel
    {
        [Key]
        public long DT_RowId { get; set; }
        public string? UserName { get; set; }
        public string? MFI { get; set; }
        public long UserID { get; set; }
        public long PartnerID { get; set; }
    }

    public class MFIList
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    public class UserMFIMapperCreateAndEditModel
    {
        public long ID { get; set; }
        public List<long> UserID { get; set; }
        public List<long> MFI { get; set; }
        public string Operation { get; set; }
    }

    public class USerMFIMapperIDs
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public long PartnerID { get; set; }
    }
}
