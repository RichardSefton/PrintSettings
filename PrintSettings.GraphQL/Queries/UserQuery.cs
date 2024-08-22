using GraphQL;
using GraphQL.Types;
using PrintSettings.Data.Services;

namespace PrintSettings.GraphQL;

public  class UserQuery : ObjectGraphType {
    public UserQuery(UserService userService) {
        Field<UserType>("user")
            .Argument<StringGraphType>("id", "The ID of the user")
            .Argument<StringGraphType>("email", "The email of the user")
            .ResolveAsync(async context => {
                var id = context.GetArgument<string>("id");
                var email = context.GetArgument<string>("email");
                if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(email)) {
                    context.Errors.Add(new ExecutionError("You must provide an ID or an email"));
                    return null;
                }
                if (!string.IsNullOrEmpty(id))
                    return await userService.GetAsync(id, UserService.UserSearchType.Id);

                if (!string.IsNullOrEmpty(email))
                    return await userService.GetAsync(email, UserService.UserSearchType.Email);

                return null;
            })
            .Description("Get a user by ID or email");
    }
}