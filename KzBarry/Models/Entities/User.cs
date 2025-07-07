using KzBarry.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace KzBarry.Models.Entities
{
    public class User : BaseModel
    {
        [Key]
        public Guid Id { get; set; }

        private string _email;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }

        public string PasswordHash { get; set; }

        [Required]
        public Role Role { get; set; }

        // Navigation property for refresh tokens
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
