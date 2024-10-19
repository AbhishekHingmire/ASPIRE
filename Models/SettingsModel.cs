using System.ComponentModel.DataAnnotations;

namespace IMS2.Models
{
    public class SettingsModel
    {
            public long ID { get; set; }
            public string Name { get; set; }
    }

    public class ScreenResult
    {
        [Key]
        public string? Name { get; set; }
    }
}
