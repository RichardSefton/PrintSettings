using GraphQL;
using GraphQL.Instrumentation;
using Microsoft.AspNetCore.Http;

namespace PrintSettings.GraphQL;

public class AuthenticatedFieldMiddleware : IFieldMiddleware {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedFieldMiddleware(IHttpContextAccessor httpContextAccessor) {
        _httpContextAccessor = httpContextAccessor;
    }

    public async ValueTask<object?> ResolveAsync(IResolveFieldContext context, FieldMiddlewareDelegate next) {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true) {
            context.Errors.Add(new ExecutionError("Not Authenticated"));
            return null;
        }
        
        return await next(context);
    }
}