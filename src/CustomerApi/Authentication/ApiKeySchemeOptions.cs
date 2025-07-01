using Microsoft.AspNetCore.Authentication;

public class ApiKeySchemeOptions : AuthenticationSchemeOptions
{
    public string HeaderName { get; } = "x-api-key";
    public string? ApiKey { get; set; }
    public bool ReadOnly { get; set; } = true;

    public override void Validate()
    {
        if (string.IsNullOrEmpty(HeaderName))
        {
            throw new ArgumentException("Header name must be provided.");
        }

        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new ArgumentException("API key must be provided.");
        }
    }
}