using PrintSettings.Models;
using PrintSettings.Data.Services;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace PrintSettings.GraphQL;

public class AuthType : ObjectGraphType<Auth> {
    public AuthType() {
        Field<BooleanGraphType>("authenticated")
            .Description("Whether the user is authenticated or not");

        Field<StringGraphType>("accessToken")
            .Description("The access token for the user");

        Field<StringGraphType>("refreshToken")
            .Description("The refresh token for the user");

        Field<UserType>("user")
            .Description("The user associated with the authentication")
            .ResolveAsync(async context => {
                if (context.RequestServices == null) 
                    return null;
                UserService userService = context.RequestServices.GetRequiredService<UserService>();
                return await userService.GetAsync(context.Source?.User?.Id ?? "", UserService.UserSearchType.Id);
            });
    }
}