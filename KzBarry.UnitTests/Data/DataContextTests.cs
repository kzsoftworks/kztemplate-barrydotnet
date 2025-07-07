
using KzBarry.Models.Entities;
using KzBarry.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using KzBarry.Models.Enums;
using FluentAssertions;

namespace KzBarry.UnitTests.Data
{
    public class DataContextTests
    {
        private DataContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new DataContext(options);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldSetCreatedAtAndUpdatedAt_OnInsertAndUpdate()
        {
            // Arrange
            const string initialEmail = "audit@a.com";
            const string updatedEmail = "changed@a.com";
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateContext(dbName);
            var user = new User { Id = Guid.NewGuid(), Email = initialEmail, PasswordHash = "ph", Role = Role.User };

            // Act - Insert
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Assert - Insert
            user.CreatedAt.Should().NotBe(default);
            user.UpdatedAt.Should().NotBe(default);
            user.Email.Should().Be(initialEmail);

            // Act - Update
            var oldUpdated = user.UpdatedAt;
            user.Email = updatedEmail;
            await context.SaveChangesAsync();

            // Assert - Update
            user.UpdatedAt.Should().BeAfter(oldUpdated);
            user.Email.Should().Be(updatedEmail);
        }
    }
}
