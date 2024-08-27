using Xunit;
using PrintSettings.Models;
using Xunit.Abstractions;
using MongoDB.Driver;
using System.Text;
using System.Text.Json;
using PrintSettings.Tests.Helpers;
using System.Net;
using System.Net.Http.Headers;

namespace PrintSettings.Tests.GraphQL;

public class UserGraphTests : IClassFixture<GraphQLFactory> {
    private readonly ITestOutputHelper _output;
    private readonly GraphQLFactory _graphQLFactory;
    private readonly IMongoCollection<User> _userCollection;

    public UserGraphTests(GraphQLFactory graphQLFactory, ITestOutputHelper output) {
        _output = output;
        _graphQLFactory = graphQLFactory;
        _graphQLFactory.Output = output;
        _userCollection = _graphQLFactory._userCollection;
    }

    [Fact]
    public async Task UserReturnsValidUserById() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        string query = @"
                query GetUser($id: String!) {
                    user(id: $id) {
                        id
                        email
                    }
                }
            ";

        var requestBody = new {
            query,
            variables = new {
                id = user.Id
            }
        };
        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        
        (string authToken, string refreshToken) = await _graphQLFactory.GenerateAuthToken(user);
        HttpClient client = _graphQLFactory.CreateClient(true, authToken, refreshToken);
        var response = await client.PostAsync("/graphql", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        var data = json.GetProperty("data");
        var userJson = data.GetProperty("user");

        Assert.Equal(user.Id, userJson.GetProperty("id").GetString());
        Assert.Equal(user.Email, userJson.GetProperty("email").GetString());
    }

    [Fact]
    public async Task UserReturnsValidUserByEmail() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Email);

        string query = @"
                query GetUser($email: String!) {
                    user(email: $email) {
                        id
                        email
                    }
                }
            ";

        var requestBody = new {
            query,
            variables = new {
                email = user.Email
            }
        };
        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        
        (string authToken, string refreshToken) = await _graphQLFactory.GenerateAuthToken(user);
        HttpClient client = _graphQLFactory.CreateClient(true, authToken, refreshToken);
        var response = await client.PostAsync("/graphql", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        var data = json.GetProperty("data");
        var userJson = data.GetProperty("user");

        Assert.Equal(user.Id, userJson.GetProperty("id").GetString());
        Assert.Equal(user.Email, userJson.GetProperty("email").GetString());
    }

    [Fact]
    public async Task UserReturnsNullForInvalidId() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Id);

        string query = @"
                query GetUser($id: String!) {
                    user(id: $id) {
                        id
                        email
                    }
                }
            ";

        var requestBody = new {
            query,
            variables = new {
                id = "123"
            }
        };
        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        
        (string authToken, string refreshToken) = await _graphQLFactory.GenerateAuthToken(user);
        HttpClient client = _graphQLFactory.CreateClient(true, authToken, refreshToken);
        var response = await client.PostAsync("/graphql", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        var data = json.GetProperty("data");
        var userJson = data.GetProperty("user");

        Assert.True(userJson.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task UserReturnsNullForInvalidEmail() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Email);

        string query = @"
                query GetUser($email: String!) {
                    user(email: $email) {
                        id
                        email
                    }
                }
            ";

        var requestBody = new {
            query,
            variables = new {
                email = "empty"
            }
        };

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);

        (string authToken, string refreshToken) = await _graphQLFactory.GenerateAuthToken(user);
        HttpClient client = _graphQLFactory.CreateClient(true, authToken, refreshToken);
        var response = await client.PostAsync("/graphql", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        var data = json.GetProperty("data");
        var userJson = data.GetProperty("user");

        Assert.True(userJson.ValueKind == JsonValueKind.Null);
    }  

    [Fact]
    public async Task UserReturnsNullForWrongAuthenticatedUser() {
        List<User> users = await Scaffold.Up(_userCollection);
        User user = users[0];
        Assert.NotNull(user);
        Assert.NotNull(user.Email);

        User user2 = users[1];
        Assert.NotNull(user2);
        Assert.NotNull(user2.Email);

        string query = @"
                query GetUser($email: String!) {
                    user(email: $email) {
                        id
                        email
                    }
                }
            ";

        var requestBody = new {
            query,
            variables = new {
                email = user.Email
            }
        };

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);

        (string authToken, string refreshToken) = await _graphQLFactory.GenerateAuthToken(user2);
        HttpClient client = _graphQLFactory.CreateClient(true, authToken, refreshToken);
        var response = await client.PostAsync("/graphql", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        string responseString = await response.Content.ReadAsStringAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseString);
        var data = json.GetProperty("data");
        var userJson = data.GetProperty("user");
        var userId = userJson.GetProperty("id");
        var userEmail = userJson.GetProperty("email");

        Assert.True(userId.ValueKind == JsonValueKind.Null);
        Assert.True(userEmail.ValueKind == JsonValueKind.Null);
    }              
}