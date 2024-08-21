namespace PrintSettings.Data.Settings;

public class JwtSettings {
    public string? Secret { get; set; } = null;
    public string? Issuer { get; set; } = null;
    public string? Audience { get; set; } = null;
    public int AccessTokenExpiration { get; set; } = 0;
    public int RefreshTokenExpiration { get; set; } = 0;
}