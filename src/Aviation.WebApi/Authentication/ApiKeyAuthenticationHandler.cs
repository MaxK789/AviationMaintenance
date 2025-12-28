using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Aviation.WebApi.Authentication;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-API-KEY";
    public const string QueryName = "api_key"; // для SignalR у браузері

    private readonly IConfiguration _cfg;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration cfg)
        : base(options, logger, encoder, clock)
    {
        _cfg = cfg;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var expected = _cfg["Security:ApiKey"];
        if (string.IsNullOrWhiteSpace(expected))
            return Task.FromResult(AuthenticateResult.Fail("API key is not configured"));

        // 1) header (REST/GraphQL)
        string? provided = null;
        if (Request.Headers.TryGetValue(HeaderName, out var hv))
            provided = hv.ToString();

        // 2) query (SignalR/WebSocket)
        if (string.IsNullOrWhiteSpace(provided))
            provided = Request.Query[QueryName].ToString();

        if (string.IsNullOrWhiteSpace(provided))
            return Task.FromResult(AuthenticateResult.Fail("Missing API key"));

        // constant-time compare
        var left = Encoding.UTF8.GetBytes(provided);
        var right = Encoding.UTF8.GetBytes(expected);
        var ok = left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);

        if (!ok)
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyClient") };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
