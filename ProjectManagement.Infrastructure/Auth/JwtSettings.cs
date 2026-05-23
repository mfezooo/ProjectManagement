namespace ProjectManagement.Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpirationMinutes { get; set; } = 60;
}
