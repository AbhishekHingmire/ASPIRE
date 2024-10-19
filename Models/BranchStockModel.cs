using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class BranchStockModel
    {
        [Key]
        public string SlNo { get; set; }      
        public string Branch { get; set; }    
        public string Code { get; set; }     
        public string Item { get; set; }     
        public string SKU { get; set; }      
        public int Qty { get; set; }     
        public string DateTimeStamp { get; set; } 
    }

    public class BranchDetails
    {
        public long ID { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long PartnerID { get; set; }
        public long BranchTypeID { get; set; }
    }

    public class BranchHierarchy
    {
        [Key]
        public long SubBranchID { get; set; }
        public long BranchID { get; set; }
    }
}
