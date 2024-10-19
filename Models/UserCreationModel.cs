using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class UserCreationModel
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
    }

    public class UserCreationGetDetailsModel
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
    }

    public class RoleModel
    {
        public string? ID { get; set; }
        public string? Name { get; set; }
    }

}
