using AutoMapper;
using FluentAssertions;
using KzBarry.Models.DTOs.Users;
using KzBarry.Models.Entities;
using KzBarry.Models.Enums;
using KzBarry.Repositories;
using KzBarry.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace KzBarry.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock = new();
        private readonly IMapper _mapper;
        private readonly Mock<IPasswordHasher<User>> _hasherMock = new();
        private readonly UserService _service;

        public UserServiceTests()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(KzBarry.Utils.Profiles.UserProfile).Assembly);
            });
            _mapper = mapperConfig.CreateMapper();
            _service = new UserService(_repoMock.Object, _mapper, _hasherMock.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsMappedDtos()
        {
            var users = new List<User> { new User { Id = Guid.NewGuid(), Email = "a@b.com", Role = Role.User } };
            var dtos = new List<UserDto> { new UserDto { Id = users[0].Id, Email = "a@b.com", Role = Role.User } };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            var result = await _service.GetUsers();
            result.Should().ContainSingle().Which.Email.Should().Be("a@b.com");
        }

        [Fact]
        public async Task GetUser_ReturnsMappedDto()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "b@b.com", Role = Role.User };
            var dto = new UserDto { Id = user.Id, Email = "b@b.com", Role = Role.User };
            _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            var result = await _service.GetUser(user.Id);
            result.Email.Should().Be("b@b.com");
        }

        [Fact]
        public async Task GetUser_ShouldThrowKeyNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var missingId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _service.GetUser(missingId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateUser_ReturnsMappedDto()
        {
            var dto = new UserCreateDto { Email = "c@b.com", Password = "pw", Role = Role.User };
            var user = new User { Id = Guid.NewGuid(), Email = "c@b.com", Role = Role.User };
            var created = new User { Id = user.Id, Email = "c@b.com", Role = Role.User };
            var mappedDto = new UserDto { Id = user.Id, Email = "c@b.com", Role = Role.User };
            _hasherMock.Setup(h => h.HashPassword(user, dto.Password)).Returns("hashed");
            _repoMock.Setup(r => r.AddAsync(user)).ReturnsAsync(created);
            _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
            var result = await _service.CreateUser(dto);
            result.Email.Should().Be("c@b.com");
        }

        [Fact]
        public async Task CreateUser_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var existingUser = new User { Id = Guid.NewGuid(), Email = "c@b.com", Role = Role.User };
            _repoMock.Setup(r => r.GetByEmailAsync(existingUser.Email)).ReturnsAsync(existingUser);

            var dto = new UserCreateDto { Email = "c@b.com", Password = "pw", Role = Role.User };

            // Act
            Func<Task> act = async () => await _service.CreateUser(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateUser_ThrowsIfNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User)null);
            Func<Task> act = () => _service.UpdateUser(Guid.NewGuid(), new UserUpdateDto());
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteUser_ShouldThrowKeyNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var missingId = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(missingId)).ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _service.DeleteUser(missingId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
            _repoMock.Verify(r => r.Remove(It.IsAny<User>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteUser_RemovesUser()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "del@b.com", Role = Role.User };
            _repoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);
            await _service.DeleteUser(user.Id);
            _repoMock.Verify(r => r.Remove(user), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
        }
    }
}
