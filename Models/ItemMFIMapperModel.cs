using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class ItemMFIMapperModel
    {
        [Key]
        public long DT_RowId { get; set; }
        public long ItemID { get; set; }
        public string? Code { get; set; }
        public string? Item { get; set; }
        public decimal MRP { get; set; }
        public decimal Price { get; set; }
        public decimal GSTPercent { get; set; }
        public bool IsLMD { get; set; }
    }

    public class ItemCBO
    {
        public long ID { get; set; }
        public string? Name { get; set; }
    }

    public class ItemMFIData
    {
        public long DT_RowId { get; set; }
        public long ID { get; set; }            
        public long ItemID { get; set; }        
        public decimal MRP { get; set; }        
        public decimal Price { get; set; }      
        public decimal GSTPercent { get; set; } 
        public bool IsLMD { get; set; } 
    }

    public class PartnerItems
    {
        [Key]
        public long ID { get; set; }
        public long PartnerID { get; set; }
        public long ItemID { get; set; }
    }
}
