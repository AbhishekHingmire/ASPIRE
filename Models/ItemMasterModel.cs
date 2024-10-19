using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class ItemMasterModel
    {
        [Key]
        public long ID { get; set; }
        public long CategoryID { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ShortDesc { get; set; }
        public string? HSNCode { get; set; }
    }

    public class GetItemMasterModel
    {
        [Key]
        public long DT_RowId { get; set; }
        public long CategoryID { get; set; }
        public string? Category { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ShortDesc { get; set; }
        public string? HSNCode { get; set; }
    }

    public class Category
    {
        [Key]
        public long ID { get; set; }
        public string? Name { get; set; }
    }
    public class ItemMasters
    {
        [Key]
        public long DT_RowId { get; set; }
        public string? Code { get; set; }
    }
}
