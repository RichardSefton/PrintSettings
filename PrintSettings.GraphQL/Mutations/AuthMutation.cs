using System.Security.Claims;
using GraphQL;
using GraphQL.Types;
using PrintSettings.Data.Services;
using PrintSettings.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace PrintSettings.GraphQL;

public class AuthMutation : ObjectGraphType {
    public AuthMutation(UserService userService, TokenService tokenService) {
        Field<AuthType>("login")
            .Argument<NonNullGraphType<StringGraphType>>("email", "The email of the user")
            .Argument<NonNullGraphType<StringGraphType>>("password", "The password of the user")
            .ResolveAsync(async context => {
                string email = context.GetArgument<string>("email");
                string password = context.GetArgument<string>("password");

                User? user = await userService.LoginAsync(email, password);

                if (user == null) {
                    context.Errors.Add(new ExecutionError("Invalid email or password"));
                    return null;
                }

                var accessClaims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user?.Id ?? ""),
                    new Claim("UserId", user?.Id ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var refreshClaims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user?.Id ?? ""),
                    new Claim("UserId", user?.Id ?? ""),
                    new Claim("Refresh", "true"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                
                var accessToken = tokenService.GenerateAccessToken(accessClaims);
                var options = new CookieOptions {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddMinutes(24)
                };
                HttpContextAccessor? httpContextAccessor = (HttpContextAccessor?)context?.UserContext?["HttpContextAccessor"];
                HttpContext? httpContext = httpContextAccessor?.HttpContext;
                if (httpContext == null) {
                    context?.Errors.Add(new ExecutionError("No HttpContext"));
                    return null;
                }
                httpContext.Response.Cookies.Append("refreshToken", tokenService.GenerateRefreshToken(refreshClaims), options);
                return new Auth(true, accessToken, user);
            });
    }
}