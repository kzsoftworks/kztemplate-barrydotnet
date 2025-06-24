using AutoMapper;
using KzBarry.Models.DTOs.Users;
using KzBarry.Models.Entities;
using KzBarry.Repositories;
using Microsoft.AspNetCore.Identity;

namespace KzBarry.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(IUserRepository userRepository,
            IMapper mapper,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var users = await _userRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("User not found.");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUser(UserCreateDto newUser)
        {
            await EnsureEmailIsUnique(newUser.Email);

            var user = _mapper.Map<User>(newUser);
            user.PasswordHash = _passwordHasher.HashPassword(user, newUser.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateUser(Guid id, UserUpdateDto updatedUser)
        {
            await EnsureEmailIsUnique(updatedUser.Email, id);

            var existingUser = await _userRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("User not found.");

            _mapper.Map(updatedUser, existingUser);
            if (!string.IsNullOrEmpty(updatedUser.Password))
                existingUser.PasswordHash = _passwordHasher.HashPassword(existingUser, updatedUser.Password);

            _userRepository.Update(existingUser);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteUser(Guid id)
        {
            var existingUser = await _userRepository.GetByIdAsync(id) 
                ?? throw new KeyNotFoundException("User not found.");

            _userRepository.Remove(existingUser);
            await _userRepository.SaveChangesAsync();
        }

        private async Task EnsureEmailIsUnique(string email, Guid? ignoreUserId = null)
        {
            var userWithSameEmail = await _userRepository.GetByEmailAsync(email);
            if (userWithSameEmail != null && userWithSameEmail.Id != ignoreUserId)
                throw new ArgumentException("Email already exists.");
        }
    }
}
