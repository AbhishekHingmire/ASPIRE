using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class FailedRecord
    {
        [Key]
        public string? LoanAppNo { get; set; }
        public string? Reason { get; set; }
        public string? OrderNo { get; set; }
    }
}
