using Microsoft.AspNetCore.TestHost;
using MongoDB.Driver;
using Mongo2Go;
using PrintSettings.Models;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using PrintSettings.GraphQL;
using PrintSettings.Data.Settings;
using PrintSettings.Data.Services;
using GraphQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace PrintSettings.Tests.Helpers;

public class GraphQLFactory : IDisposable {
    private readonly MongoDbRunner _mongoRunner;
    public IMongoCollection<User> _userCollection;
    public ITestOutputHelper? Output { get; set; }

    private TestServer _server;

    public GraphQLFactory() {
        _mongoRunner = MongoDbRunner.Start();
        var client = new MongoClient(_mongoRunner.ConnectionString);
        var database = client.GetDatabase("PrintSettings");
        _userCollection = database.GetCollection<User>("Users");

        _server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services => {
                services.AddHttpContextAccessor();
                
                services.Configure<PrintSettingsDatabaseSettings>(options => {
                    options.ConnectionString = _mongoRunner.ConnectionString;
                    options.DatabaseName = "PrintSettings";
                    options.UserCollectionName = "Users"; 
                });

                services.Configure<JwtSettings>(options => {
                    options.Secret = "YourSuperSecretKeyGoesHereWhichIsAtLeast32Chars";
                    options.Issuer = "PrintSettings";
                    options.Audience = "PrintSettings";
                    options.AccessTokenExpiration = 30;
                    options.RefreshTokenExpiration = 7;
                });

                services.AddAuthentication(options => {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "PrintSettings",
                        ValidAudience = "PrintSettings",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyGoesHereWhichIsAtLeast32Chars"))
                    };
                    options.Events = new JwtBearerEvents {
                        OnMessageReceived = context => {
                            context.Token = context.Request.Cookies["access_token"];
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context => {
                            var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                            var userClaim = context?.Principal?.Claims?.FirstOrDefault(c => c.Type == "UserId");
                            User? user = await userService.GetAsync(userClaim?.Value ?? "", UserService.UserSearchType.Id);
                            if (user == null) {
                                context?.Fail("Unauthorized");
                            }
                            context?.HttpContext?.Items.Add("User", user);
                        }
                    };
                });

                services.AddAuthorization(options => {
                    options.AddPolicy("JWT", policy => {
                        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                    });
                });

                services.AddRouting();

                services.AddScoped<UserService>();
                services.AddScoped<TokenService>();

                services.AddPrintSettingsServices();
            })
            .Configure(app => {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => {
                    endpoints.MapGraphQL();
                });
            })
        ); 
    }

    public HttpClient CreateClient(bool authenticated, string token, string refreshToken = "") {
        var client = _server.CreateClient();
        if (authenticated) {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (!string.IsNullOrEmpty(refreshToken)) {
            client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken}");
        }

        return client;
    }

    public async Task<(string, string)> GenerateAuthToken(User user) {
        string mutation = @"
            mutation Login($email: String!, $password: String!) {
                login(email: $email, password: $password) {
                    authenticated
                    accessToken
                    refreshToken
                }
            }";

        var variables = new {
            email = user.Email,
            password = "SomePass4NAV!"
        };

        var requestBody = new {
            query = mutation,
            variables = variables
        };
        
        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        HttpClient client = CreateClient(false, "");
        var response = await client.PostAsync("/graphql", new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);

        string token = json.GetProperty("data")
            .GetProperty("login")
            .GetProperty("accessToken")
            .GetString() ?? "";

        string refreshToken = response?.Headers?.GetValues("Set-Cookie")?.FirstOrDefault()?.Split(";")[0].Split("=")[1] ?? "";

        return (token, refreshToken);
    }

    public void Dispose() {
        _mongoRunner.Dispose();
    }
}