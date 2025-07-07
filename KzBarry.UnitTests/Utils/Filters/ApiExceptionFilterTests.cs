using FluentAssertions;
using KzBarry.Utils.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KzBarry.UnitTests.Utils.Filters
{
    public class ApiExceptionFilterTests
    {
        [Fact]
        public void OnException_HandlesArgumentNullException_AsBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApiExceptionFilter>>();
            var filter = new ApiExceptionFilter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new ArgumentNullException("param", "msg")
            };

            // Act
            filter.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue();
            var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void OnException_HandlesKeyNotFoundException_AsNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApiExceptionFilter>>();
            var filter = new ApiExceptionFilter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new KeyNotFoundException("not found")
            };

            // Act
            filter.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue();
            var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public void OnException_HandlesUnhandledException_AsInternalServerError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApiExceptionFilter>>();
            var filter = new ApiExceptionFilter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new Exception("fail")
            };

            // Act
            filter.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue();
            var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(500);
        }
        [Fact]
        public void OnException_HandlesArgumentException_AsBadRequest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApiExceptionFilter>>();
            var filter = new ApiExceptionFilter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new ArgumentException("argument invalid")
            };

            // Act
            filter.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue();
            var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void OnException_HandlesUnauthorizedAccessException_AsUnauthorized()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApiExceptionFilter>>();
            var filter = new ApiExceptionFilter(loggerMock.Object);
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = new UnauthorizedAccessException("not authorized")
            };

            // Act
            filter.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue();
            var result = context.Result.Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(401);
        }
    }
}

