namespace backend.Config;

public class JwtConfig
{
    public const string Jwt = "Jwt";
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required int AccessTTL { get; set; }
}

public class SSOConfig
{
    public const string SSO = "SSO";
    public required string Service { get; set; }
    public required string Authority { get; set; }
}
