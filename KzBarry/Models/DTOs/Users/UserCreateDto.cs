using System.ComponentModel.DataAnnotations;
using KzBarry.Models.Enums;

namespace KzBarry.Models.DTOs.Users
{
    public class UserCreateDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }
    }
}
