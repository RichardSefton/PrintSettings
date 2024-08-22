using GraphQL.Types;

namespace PrintSettings.GraphQL;

public class RootQuery : ObjectGraphType {
    public RootQuery(UserQuery userQuery) {
        foreach (var field in userQuery.Fields) {
            AddField(field);
        }
    }
}