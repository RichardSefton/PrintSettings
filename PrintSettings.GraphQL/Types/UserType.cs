using PrintSettings.Models;
using PrintSettings.Data.Services;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace PrintSettings.GraphQL;

public class UserType : ObjectGraphType<User> {
    public UserType() {
        Field<StringGraphType>("id")
            .UserHasAccess()
            .Description("The ID of the user");

        Field<StringGraphType>("email")
            .UserHasAccess()
            .Description("The email of the user");
    }
}