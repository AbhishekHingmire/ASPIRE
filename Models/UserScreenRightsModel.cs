using System.ComponentModel.DataAnnotations.Schema;

namespace IMS2.Models
{
    public class UserScreenRightsModel
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public List<long> ScreenIds { get; set; }
        public List<long> ScreenRightsIds { get; set; }
    }

    public class UpdateUserScreenRightsModel
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public long ScreenID { get; set; }
        public long ScreenRightsID { get; set; }
    }

    public class ScreenRightsModel
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public long ScreenID { get; set; }
        public long ScreenRightsID { get; set; }
    }

    public class UserScreenRightsViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public long UserID { get; set; }
        public string Screen { get; set; }
        public long ScreenID { get; set; }
        public string Rights { get; set; }
        public long ScreenRightsID { get; set; }
    }

    public class UserScreenRight
    {

        public long ID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

    }

    public class UserScreen
    {

        public long ID { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }

    }

    public class UserDetails
    {

        public long ID { get; set; }
        public string? Username { get; set; }
    }

}
