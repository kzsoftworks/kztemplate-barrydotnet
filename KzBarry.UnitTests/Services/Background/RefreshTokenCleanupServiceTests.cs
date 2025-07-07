using System.Threading;
using System.Threading.Tasks;
using KzBarry.Services.Background;
using Xunit;
using Moq;
using KzBarry.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace KzBarry.UnitTests.Services.Background
{
    public class RefreshTokenCleanupServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_CallsDeleteExpiredAsync_WhenEnabled()
        {
            // Arrange
            var repoMock = new Mock<IRefreshTokenRepository>();
            repoMock.Setup(r => r.DeleteExpiredAsync()).ReturnsAsync(1);
            var scopeMock = new Mock<IServiceScope>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(p => p.GetService(typeof(IRefreshTokenRepository))).Returns(repoMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            providerMock.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            var loggerMock = new Mock<ILogger<RefreshTokenCleanupService>>();
            var options = Options.Create(new RefreshTokenCleanupOptions { Enabled = true, IntervalMinutes = 0.001 });
            var service = new RefreshTokenCleanupService(providerMock.Object, options, loggerMock.Object);

            // Act
            var cts = new CancellationTokenSource(50);
            await service.StartAsync(cts.Token);

            // Assert
            repoMock.Verify(r => r.DeleteExpiredAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_LogsAndContinues_OnException()
        {
            // Arrange
            var repoMock = new Mock<IRefreshTokenRepository>();
            repoMock.Setup(r => r.DeleteExpiredAsync()).ThrowsAsync(new Exception("fail"));
            var scopeMock = new Mock<IServiceScope>();
            var providerMock = new Mock<IServiceProvider>();
            providerMock.Setup(p => p.GetService(typeof(IRefreshTokenRepository))).Returns(repoMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
            providerMock.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);
            var loggerMock = new Mock<ILogger<RefreshTokenCleanupService>>();
            var options = Options.Create(new RefreshTokenCleanupOptions { Enabled = true, IntervalMinutes = 0.001 });
            var service = new RefreshTokenCleanupService(providerMock.Object, options, loggerMock.Object);

            // Act
            var cts = new CancellationTokenSource(50);
            await service.StartAsync(cts.Token);

            // Assert
            loggerMock.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.AtLeastOnce);
        }
    }
}
