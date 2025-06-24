using System.ComponentModel.DataAnnotations;

namespace KzBarry.Models.Entities
{
    public interface IAuditable
    {
        [Required]
        DateTime CreatedAt { get; set; }
        [Required]
        DateTime UpdatedAt { get; set; }
    }
}
