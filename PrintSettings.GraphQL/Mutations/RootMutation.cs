using GraphQL.Types;

namespace PrintSettings.GraphQL;

public class RootMutation : ObjectGraphType {
    public RootMutation(UserMutation userMutation, AuthMutation authMutation) {
        foreach (var field in userMutation.Fields) {
            AddField(field);
        }

        foreach (var field in authMutation.Fields) {
            AddField(field);
        }
    }
}