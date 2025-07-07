using System.ComponentModel.DataAnnotations;
using KzBarry.Models.Enums;

namespace KzBarry.Models.DTOs.Users
{
    public class UserUpdateDto
    {
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        [Required]
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }
    }
}
