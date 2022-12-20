using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBookWeb.Models
{
    public class Category
    {
        [Key] // Id'yi otomatik artan ve primary key yaptık.
        public int Id { get; set; }
        [Required] // Name null olamaz.
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100 only")]
        public int DisplayOrder { get; set; }
        
        // Sayfada doğru şekilde gözükmesi için yazdığımız kod
        public DateTime CreatedDateTime { get; set; } = DateTime.Now; // default value ayarladık.
    }
}
