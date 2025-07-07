using System.ComponentModel.DataAnnotations;

namespace KzBarry.Models.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
