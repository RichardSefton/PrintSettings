using PrintSettings.Models;
using PrintSettings.Data.Services;
using GraphQL;
using GraphQL.Types;

namespace PrintSettings.GraphQL;

public class UserMutation : ObjectGraphType {
    public UserMutation(UserService userService) {
        Field<UserType>("addUser")
            .Argument<NonNullGraphType<StringGraphType>>("email", "The email of the user")
            .Argument<NonNullGraphType<StringGraphType>>("password", "The password of the user")
            .ResolveAsync(async context => {
                var email = context.GetArgument<string>("email");
                var password = context.GetArgument<string>("password");
                try {
                    User user = new(email, password);
                    user.SetPassword(password);

                    return await userService.CreateAsync(user);
                } catch (Exception ex) {
                    context.Errors.Add(new ExecutionError(ex.Message));
                    return null;
                }
            })
            .Description("Add a new user");
    }
}