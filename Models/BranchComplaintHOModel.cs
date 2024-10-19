using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class BranchComplaintHOModel
    {
        [Key]
        public string SlNo { get; set; }
        public string ComplaintCode { get; set; }
        public string ComplaintType { get; set; }
        public string SupplierRefNo { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string AMBMContactName { get; set; }
        public string AMBMContactNo { get; set; }
        public string Item { get; set; }
        public string BranchOrCustomerAddress { get; set; }
        public string BranchOrCustomerContactNo { get; set; }
        public int NoOfDefectiveUnits { get; set; }
        public string Problem { get; set; }
        public string CreatedOn { get; set; }
        public string Status { get; set; }
        public string Resolved { get; set; } 
        public string? ResolvedOn { get; set; } 
        public string LastRemark { get; set; }
        public string? LastRemarkOn { get; set; }
    }
}
