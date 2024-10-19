using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class BranchModel
    {
        [Key]
        public long DT_RowId { get; set; }
        public long BranchTypeID { get; set; }
        public long ParentID { get; set; }
        public string? ParentBranchType { get; set; }
        public string? ParentCode { get; set; }
        public string? ParentName { get; set; }
        public string? BranchType { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public long CityID { get; set; }
        public string? City { get; set; }
        public long? StockistID { get; set; } 
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    public class BranchMasterModel
    {
        public long? DT_RowId { get; set; }
        public long? BranchTypeID { get; set; }
        public long? ParentID { get; set; }
        public long? UserID { get; set; } //
        public long? PartnerID { get; set; } // 
        public long? AdminUserID { get; set; } //
        public long? CityID { get; set; } //
        public string? Code { get; set; }
        public string? Role { get; set; } //
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }


    public class BranchListViewModel
    {
        public List<BranchModel>? Branches { get; set; }
        public List<BranchTypeModel>? BranchTypes { get; set; }
        public List<CityModel>? Cities { get; set; }
        public List<StockistModel>? Stockists { get; set; }
    }

    public class BranchTypeModel
    {
        public long ID { get; set; }
        public string? Name { get; set; }
    }

    public class CityModel
    {
        public int ID { get; set; }
        public string? Name { get; set; }
    }

    public class StockistModel
    {
        public int ID { get; set; }
        public string? Name { get; set; }
    }

    public class ParentBranchModel
    {
        public long ID { get; set; }
        public string? Name { get; set; }
    }
        
}
