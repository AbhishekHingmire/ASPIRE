using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS2.Models
{
    public class Users
    {
        public long ID { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        //[NotMapped]
        public bool IsApproved { get; set; }
        [NotMapped]
        public string? Token { get; set; }
    }

    public class UserScreenRights
    {
        [Key]
        public long ID { get; set; }
        public string? Username { get; set; }
        public string? Screen { get; set; }
        public string? Rights { get; set; }
    }
}
