using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace KzBarry.Utils.Filters
{
    // Adds 500 to all endpoints, 401 to the ones with [Authorize] and 403 to the ones with [Authorize(Roles = ...)]
    public class SwaggerStandardResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 500 for all
            if (!operation.Responses.ContainsKey("500"))
                operation.Responses.Add("500", new OpenApiResponse { Description = "Internal Server Error" });

            // 401 to endpoints with [Authorize]
            var hasAuthorize =
                context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (hasAuthorize && !operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

            // 403 to endpoints with [Authorize(Roles = ...)]
            var hasRoleRestriction =
                context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any(a => !string.IsNullOrWhiteSpace(a.Roles)) ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (hasRoleRestriction && !operation.Responses.ContainsKey("403"))
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
        }
    }
}
