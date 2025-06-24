using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace KzBarry.Utils.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        ObjectResult result;
        int statusCode;

        switch (context.Exception)
        {
            case ArgumentNullException argNullEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning(argNullEx, "ArgumentNullException: {Message}", argNullEx.Message);
                result = new ObjectResult(new { error = argNullEx.Message }) { StatusCode = statusCode };
                break;
            case ArgumentException argEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning(argEx, "ArgumentException: {Message}", argEx.Message);
                result = new ObjectResult(new { error = argEx.Message }) { StatusCode = statusCode };
                break;
            case KeyNotFoundException keyNotFoundEx:
                statusCode = (int)HttpStatusCode.NotFound;
                _logger.LogWarning(keyNotFoundEx, "KeyNotFoundException: {Message}", keyNotFoundEx.Message);
                result = new ObjectResult(new { error = keyNotFoundEx.Message }) { StatusCode = statusCode };
                break;
            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(context.Exception, "Unhandled exception: {Message}", context.Exception.Message);
                result = new ObjectResult(new { error = "Unexpected error." }) { StatusCode = statusCode };
                break;
        }

        context.Result = result;
        context.ExceptionHandled = true;
    }
}
}
