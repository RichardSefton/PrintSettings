using System.Reflection;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Resolvers;
using Microsoft.AspNetCore.Http;

namespace PrintSettings.GraphQL;

public static class FieldBuilderExtensions {
    private static List<string> _ignoreList = new List<string> { "CreateUser" };

    private static async Task<TReturnType?> ResolveNextOrFallback<TSourceType, TReturnType>(IResolveFieldContext<TSourceType>? context, IFieldResolver? next) {
        if (next != null) {
            var result = await next.ResolveAsync(context ?? throw new ArgumentNullException(nameof(context)));
            if (result != null) 
                return (TReturnType)result;
        } else {
            var fieldName = context?.FieldAst.Name.Value;
            var propertyInfo = context?.Source?.GetType().GetProperty(fieldName?.ToString() ?? "", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null && context != null) {
                var propertyValue = propertyInfo.GetValue(context.Source);
                return (TReturnType?)propertyValue;
            }
        }

        return default;
    }

    public static FieldBuilder<TSourceType, TReturnType> UserHasAccess<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder) {
        var next = builder.FieldType.Resolver;
        builder.ResolveAsync(async context => {
            if (_ignoreList.Contains(context?.Operation?.Name?.Value.ToString() ?? "")) 
                return await ResolveNextOrFallback<TSourceType, TReturnType>(context, next);

            var userContext = context?.UserContext;
            if (userContext?["HttpContextAccessor"] is not HttpContextAccessor httpContextAccessor) {
                context?.Errors.Add(new ExecutionError("Not Authenticated"));
                return default;
            } 

            var httpContext = httpContextAccessor.HttpContext;

            if (!(httpContext?.User?.Identity?.IsAuthenticated ?? false)) {
                context?.Errors.Add(new ExecutionError("Not Authenticated"));
                return default;
            }

            return await ResolveNextOrFallback<TSourceType, TReturnType>(context, next);
        });

        return builder;
    }
}