using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Encodings.Web;

//Courtesy from https://jflower.co.uk/adding-api-key-authentication-to-asp-net-core-with-authenticationhandler/
public class ApiKeyHandler : AuthenticationHandler<ApiKeySchemeOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiKeyHandler(IOptionsMonitor<ApiKeySchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IHttpContextAccessor httpContextAccessor) : base(options, logger, encoder, clock)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Retrieve the API key from the specified header name.
        StringValues apiKeys = StringValues.Empty;

        bool apiKeyPresent = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue(Options.HeaderName, out apiKeys) ?? false;

        // Return 'NoResult' if the header is not present as this is not intended for the ApiKeyHandler to handle.
        if (!apiKeyPresent)
        {
            return AuthenticateResult.NoResult();
        }

        // Ensure only one API key is provided, otherwise return 'Fail' with an error message.
        if (apiKeys.Count > 1)
        {
            return AuthenticateResult.Fail("Multiple API keys found in request. Please only provide one key.");
        }

        // Ensure the API key provided is valid.
        if (string.IsNullOrEmpty(Options.ApiKey) || !Options.ApiKey.Equals(apiKeys.FirstOrDefault()))
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        // Create a ClaimsIdentity with all the claims associated with the API key. This would usually come from a database.
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, Options.ApiKey),
            new Claim(ClaimTypes.Name, "API Key User")
        };

        if (Options.ReadOnly)
        {
            claims.Add(new Claim(ClaimTypes.Role, "ReadOnly")); ;
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "ReadWrite")); ;
        }

        // Create a ClaimsIdentity, ClaimsPrincipal and return an AuthenticationTicket for the user with the claims.
        ClaimsIdentity identity = new(claims, Scheme.Name);

        ClaimsPrincipal principal = new(identity);

        AuthenticationTicket ticket = new(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}