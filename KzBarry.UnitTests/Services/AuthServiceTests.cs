using AutoMapper;
using KzBarry.Models.DTOs.Auth;
using FluentAssertions;
using KzBarry.Models.Entities;
using KzBarry.Models.Enums;
using KzBarry.Repositories;
using KzBarry.Services;
using KzBarry.Utils.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KzBarry.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IPasswordHasher<User>> _hasherMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
        private readonly IMapper _mapper;
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly JwtHelper _jwtHelper;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            // Configuración mock para JwtHelper
            _configMock.Setup(c => c["Jwt:Key"]).Returns("clave-secreta-para-test-1234567890123456");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("test-audience");
            _configMock.Setup(c => c["Jwt:ExpiresInMinutes"]).Returns("15");

            _jwtHelper = new JwtHelper(_configMock.Object);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(KzBarry.Utils.Profiles.UserProfile).Assembly);
            });
            _mapper = mapperConfig.CreateMapper();

            _service = new AuthService(
                _userRepoMock.Object,
                _userServiceMock.Object,
                _hasherMock.Object,
                _jwtHelper,
                _refreshTokenRepoMock.Object,
                _mapper,
                _configMock.Object
            );
        }

        [Fact]
        public async Task RegisterUser_ReturnsAuthResponse()
        {
            // Arrange
            var request = new RegisterRequest { Email = "test@a.com", Password = "pw" };
            var user = new User { Id = Guid.NewGuid(), Email = "test@a.com", Role = Role.User };
            var mappedDto = new AuthResponse { Token = "jwt", RefreshToken = "refresh" };
            _userServiceMock.Setup(s => s.CreateUser(It.IsAny<Models.DTOs.Users.UserCreateDto>())).ReturnsAsync(new Models.DTOs.Users.UserDto { Id = user.Id, Email = user.Email, Role = user.Role });
            _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken { Token = "refresh" });
            _refreshTokenRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
            // Act
            var result = await _service.RegisterUser(request);
            // Assert
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_ReturnsAuthResponse_IfPasswordValid()
        {
            var request = new LoginRequest { Email = "test@a.com", Password = "pw" };
            var user = new User { Id = Guid.NewGuid(), Email = "test@a.com", PasswordHash = "hash", Role = Role.User };
            _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
            _hasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, request.Password)).Returns(PasswordVerificationResult.Success);
            
            _refreshTokenRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync(new RefreshToken { Token = "refresh" });
            _refreshTokenRepoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
            var result = await _service.Login(request);
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_ThrowsIfUserNotFound()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);
            Func<Task> act = () => _service.Login(new LoginRequest { Email = "none@a.com", Password = "pw" });
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Login_ThrowsIfPasswordInvalid()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "test@a.com", PasswordHash = "hash", Role = Role.User };
            _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _hasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, It.IsAny<string>())).Returns(PasswordVerificationResult.Failed);
            Func<Task> act = () => _service.Login(new LoginRequest { Email = user.Email, Password = "wrong" });
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Refresh_ThrowsIfTokenNotFound()
        {
            _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken)null);
            Func<Task> act = () => _service.Refresh(new RefreshRequest { RefreshToken = "bad" }, "userId");
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task Refresh_ThrowsIfUserIdDoesNotMatch()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var token = new RefreshToken { Token = "refresh", UserId = userId };
            _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("refresh")).ReturnsAsync(token);
            Func<Task> act = () => _service.Refresh(new RefreshRequest { RefreshToken = "refresh" }, otherUserId.ToString());
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
