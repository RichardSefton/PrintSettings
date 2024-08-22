using Schema = GraphQL.Types.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace PrintSettings.GraphQL;

public class RootSchema : Schema {
    public RootSchema(IServiceProvider provider) : base(provider) {
        Query = provider.GetRequiredService<RootQuery>();
        Mutation = provider.GetRequiredService<RootMutation>();
    }
}