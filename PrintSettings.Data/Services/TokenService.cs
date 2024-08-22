using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PrintSettings.Data.Settings;
using PrintSettings.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace PrintSettings.Data.Services;

public class TokenService {
    private readonly IOptions<JwtSettings> _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings) {
        _jwtSettings = jwtSettings;
    }

    public string GenerateAccessToken(Claim[] claims) {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Secret ?? ""));
        var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.Value.AccessTokenExpiration),
            signingCredentials: signinCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    public string GenerateRefreshToken(Claim[] claims) {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Secret ?? ""));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new JwtSecurityToken(
            issuer: _jwtSettings.Value.Issuer,
            audience: _jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.Value.RefreshTokenExpiration),
            signingCredentials: signinCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    public ClaimsPrincipal CheckTokenISValid(string token) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Value.Secret ?? "");
        var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _jwtSettings.Value.Issuer,
            ValidAudience = _jwtSettings.Value.Audience,
            ValidateLifetime = true
        }, out SecurityToken _);

        return claims;
    }
}