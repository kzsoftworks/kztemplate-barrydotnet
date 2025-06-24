using AutoMapper;
using KzBarry.Models.DTOs.Auth;
using KzBarry.Models.DTOs.Users;
using KzBarry.Models.Entities;
using KzBarry.Models.Enums;
using KzBarry.Repositories;
using KzBarry.Utils.Helpers;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace KzBarry.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly JwtHelper _jwtHelper;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository userRepository,
            IUserService userService,
            IPasswordHasher<User> passwordHasher,
            JwtHelper jwtHelper,
            IRefreshTokenRepository refreshTokenRepository,
            IMapper mapper,
            IConfiguration config)
        {
            _userService = userService;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtHelper = jwtHelper;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
            _config = config;
        }

        public async Task<AuthResponse> RegisterUser(RegisterRequest request)
        {
            var userCreateDto = _mapper.Map<UserCreateDto>(request);
            userCreateDto.Role = Role.User;
            var userDto = await _userService.CreateUser(userCreateDto);
            var user = _mapper.Map<User>(userDto);

            var token = _jwtHelper.GenerateToken(user);
            var refreshToken = await GenerateAndStoreRefreshToken(user);
            return new AuthResponse() { Token = token, RefreshToken = refreshToken };
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("Invalid credentials.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new ArgumentException("Invalid credentials.");

            var token = _jwtHelper.GenerateToken(user);
            var refreshToken = await GenerateAndStoreRefreshToken(user);
            return new AuthResponse() { Token = token, RefreshToken = refreshToken };
        }

        public async Task<AuthResponse> Refresh(RefreshRequest request, string userIdClaim)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (existingToken == null || existingToken.Expires < DateTime.UtcNow || existingToken.UserId.ToString() != userIdClaim)
                throw new ArgumentException("Invalid or expired refresh token.");

            _refreshTokenRepository.Remove(existingToken);
            await _refreshTokenRepository.SaveChangesAsync();

            var user = existingToken.User;
            var newJwt = _jwtHelper.GenerateToken(user);
            var newRefreshToken = await GenerateAndStoreRefreshToken(user);
            return new AuthResponse() { Token = newJwt, RefreshToken = newRefreshToken };
        }

        public async Task Logout(RefreshRequest request)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (existingToken != null)
            {
                _refreshTokenRepository.Remove(existingToken);
                await _refreshTokenRepository.SaveChangesAsync();
            }
        }

        private async Task<string> GenerateAndStoreRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateSecureToken(),
                Expires = DateTime.UtcNow.AddDays(int.TryParse(_config["Jwt:RefreshTokenExpiresInDays"], out var days) ? days : 7),
                UserId = user.Id
            };
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();
            return refreshToken.Token;
        }

        private static string GenerateSecureToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
