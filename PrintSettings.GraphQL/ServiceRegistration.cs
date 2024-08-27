using Microsoft.Extensions.DependencyInjection;
using PrintSettings.Data.Services;
using PrintSettings.Models;
using GraphQL;
using GraphQL.Types;
using MongoDB.Driver;
using System.Reflection;
using PrintSettings.Data.Settings;
using Microsoft.AspNetCore.Http;

namespace PrintSettings.GraphQL;

public static class ServiceRegistration {
    public static void AddPrintSettingsServices(this IServiceCollection services) {
        //Queries
        services.AddScoped<UserQuery>();
        services.AddScoped<RootQuery>();
        //Mutations
        services.AddScoped<UserMutation>();
        services.AddScoped<AuthMutation>();
        services.AddScoped<RootMutation>();

        services.AddScoped<ISchema, RootSchema>();

        var graphQLTypes= Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => !x.IsAbstract && (
                typeof(IObjectGraphType).IsAssignableFrom(x) ||
                typeof(IInputObjectGraphType).IsAssignableFrom(x)
            ));

        foreach (var type in graphQLTypes) {
            services.AddScoped(type);
        }

        services.AddGraphQL(b => b
            .AddSystemTextJson()
            .AddAuthorizationRule()
            .ConfigureExecutionOptions(options => {
                if (options is null) return;
                if (options.UserContext != null) {
                    var httpContext = options?.RequestServices?.GetRequiredService<IHttpContextAccessor>();
                    var userService = options?.RequestServices?.GetRequiredService<UserService>();
                    
                    if (options?.UserContext is null) return;
                    options.UserContext = new Dictionary<string, object?> {
                        { "HttpContextAccessor", httpContext },
                        { "UserService", userService }
                    };
                }
            })
        );
    }
}