namespace CustomerCustomerApi.Services;

public class GraphServiceOptions
{
    public string AzureB2CTenantId { get; set; }
    public string AzureB2CGraphAccessAppRegClientId { get; set; }
    public string AzureB2CGraphAccessAppRegClientSecret { get; set; }
    public string Issuer { get; set; }
}
