using System.ComponentModel.DataAnnotations;

namespace KzBarry.Models.Entities
{
    public class BaseModel : IAuditable
    {
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
