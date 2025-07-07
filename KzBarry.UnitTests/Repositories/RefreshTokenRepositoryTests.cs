using KzBarry.Repositories;
using Xunit;
using FluentAssertions;
using KzBarry.Models.Entities;
using KzBarry.Data;
using Microsoft.EntityFrameworkCore;
using KzBarry.Models.Enums;

namespace KzBarry.UnitTests.Repositories
{
    public class RefreshTokenRepositoryTests
    {
        private static DataContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new DataContext(options);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnTokenWithUser_WhenTokenExists()
        {
            // Arrange
            const string tokenValue = "tok123";
            const string userEmail = "a@b.com";
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateContext(dbName);
            var user = new User { Id = Guid.NewGuid(), Email = userEmail, PasswordHash = "ph", Role = Role.User };
            var token = new RefreshToken { Id = Guid.NewGuid(), Token = tokenValue, Expires = DateTime.UtcNow.AddDays(1), User = user, UserId = user.Id };
            context.Users.Add(user);
            context.RefreshTokens.Add(token);
            await context.SaveChangesAsync();
            var repo = new RefreshTokenRepository(context);

            // Act
            var result = await repo.GetByTokenAsync(tokenValue);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(tokenValue);
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(userEmail);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnNull_WhenTokenDoesNotExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateContext(dbName);
            var repo = new RefreshTokenRepository(context);

            // Act
            var result = await repo.GetByTokenAsync("notfound");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteExpiredAsync_ShouldRemoveOnlyExpiredTokens()
        {
            // Arrange
            const string expiredTokenValue = "expired";
            const string validTokenValue = "valid";
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateContext(dbName);
            var expired = new RefreshToken { Id = Guid.NewGuid(), Token = expiredTokenValue, Expires = DateTime.UtcNow.AddDays(-1), UserId = Guid.NewGuid() };
            var valid = new RefreshToken { Id = Guid.NewGuid(), Token = validTokenValue, Expires = DateTime.UtcNow.AddDays(1), UserId = Guid.NewGuid() };
            context.RefreshTokens.AddRange(expired, valid);
            await context.SaveChangesAsync();
            var repo = new RefreshTokenRepository(context);

            // Act
            var deleted = await repo.DeleteExpiredAsync();

            // Assert
            deleted.Should().Be(1);
            context.RefreshTokens.Should().ContainSingle(x => x.Token == validTokenValue);
            context.RefreshTokens.Should().NotContain(x => x.Token == expiredTokenValue);
        }

        [Fact]
        public async Task DeleteExpiredAsync_ShouldReturnZero_WhenNoExpiredTokensExist()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateContext(dbName);
            var valid = new RefreshToken { Id = Guid.NewGuid(), Token = "valid", Expires = DateTime.UtcNow.AddDays(1), UserId = Guid.NewGuid() };
            context.RefreshTokens.Add(valid);
            await context.SaveChangesAsync();
            var repo = new RefreshTokenRepository(context);

            // Act
            var deleted = await repo.DeleteExpiredAsync();

            // Assert
            deleted.Should().Be(0);
            context.RefreshTokens.Should().ContainSingle(x => x.Token == "valid");
        }

        [Fact]
        public async Task DeleteExpiredAsync_ShouldRemoveAllTokens_WhenAllAreExpired()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateContext(dbName);
            var expired1 = new RefreshToken { Id = Guid.NewGuid(), Token = "exp1", Expires = DateTime.UtcNow.AddDays(-2), UserId = Guid.NewGuid() };
            var expired2 = new RefreshToken { Id = Guid.NewGuid(), Token = "exp2", Expires = DateTime.UtcNow.AddDays(-1), UserId = Guid.NewGuid() };
            context.RefreshTokens.AddRange(expired1, expired2);
            await context.SaveChangesAsync();
            var repo = new RefreshTokenRepository(context);

            // Act
            var deleted = await repo.DeleteExpiredAsync();

            // Assert
            deleted.Should().Be(2);
            context.RefreshTokens.Should().BeEmpty();
        }
    }
}
