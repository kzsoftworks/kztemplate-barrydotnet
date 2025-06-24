using KzBarry.Models.DTOs.Users;

namespace KzBarry.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsers();
        Task<UserDto> GetUser(Guid id);
        Task<UserDto> CreateUser(UserCreateDto newUser);
        Task UpdateUser(Guid id, UserUpdateDto updatedUser);
        Task DeleteUser(Guid id);
    }
}
