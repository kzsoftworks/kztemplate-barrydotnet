using KzBarry.Models.Enums;

namespace KzBarry.Models.DTOs.Users
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
    }
}
