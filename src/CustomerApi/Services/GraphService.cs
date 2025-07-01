using Azure.Identity;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.User;
using Microsoft.Graph;
using User = Microsoft.Graph.User;

namespace CustomerCustomerApi.Services;

public class GraphService : IGraphService
{
    private GraphServiceClient _graphServiceClient;
    private readonly ILogger<GraphService> _logger;
    private readonly string Issuer;
    private readonly GraphServiceOptions _gsOptions;
    public GraphService(GraphServiceOptions gsOptions, ILogger<GraphService> logger)
    {
        _gsOptions = gsOptions;
        _logger = logger;
        Issuer = gsOptions.Issuer;

        string[] scopes = new[] { ".default" };
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };
        var clientSecretCredential = new ClientSecretCredential(gsOptions.AzureB2CTenantId, gsOptions.AzureB2CGraphAccessAppRegClientId, gsOptions.AzureB2CGraphAccessAppRegClientSecret, options);
        _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
        _logger.LogInformation($"Created GraphServiceClient with AzureB2CTenantId: {gsOptions.AzureB2CTenantId}, " +
            $"AzureB2CGraphAccessAppRegClientId: {gsOptions.AzureB2CGraphAccessAppRegClientId}, AzureB2CGraphAccessAppRegClientId: {_gsOptions.AzureB2CGraphAccessAppRegClientId}");
    }
    public async Task<User> CreateUserAsync(MetisUser user)
    {

        var tempPass = Guid.NewGuid().ToString();
        var userEntry = new User()
        {
            AccountEnabled = true,
            DisplayName = user.DisplayName,
            PasswordProfile = new PasswordProfile
            {
                ForceChangePasswordNextSignIn = true,
                Password = tempPass
            },
            Identities = new List<ObjectIdentity>
                    {
                        new ObjectIdentity()
                        {
                            SignInType = "emailAddress",
                            Issuer = Issuer,
                            IssuerAssignedId = user.Email
                        }
                    }
        };
        try
        {
            var graphUser = await _graphServiceClient.Users.Request().AddAsync(userEntry);
            graphUser.PasswordProfile = new PasswordProfile()
            {
                Password = tempPass,
                ForceChangePasswordNextSignIn = true
            };
            return graphUser;
        }
        catch (Exception e)
        {
            throw new GraphServiceException($"Could not create user in Azure Graph User. Ensure the MS Graph API Credentials are correct. " +
                $"User Display Name: {user.DisplayName}, Customer: {user.CustomerNumber}, B2C TenantId: {_gsOptions.AzureB2CTenantId}, AzureB2CGraphAccessAppRegClientId: {_gsOptions.AzureB2CGraphAccessAppRegClientId}", e);
        }

    }

    public async Task<bool> GetB2CUserRoles()
    {
        try
        {
            var directoryRoles = await _graphServiceClient.DirectoryRoles
          .Request()
          .GetAsync();

            var users =await  _graphServiceClient.Users.Request().GetAsync();

            var requestBody = new DirectoryObject
            {
                Id = "2456a78d-fcb6-4161-9e42-9073cd30c16c"
            };

            // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
            await _graphServiceClient.DirectoryRoles["roleTemplateId=62e90394-69f5-4237-9190-012177145e10"].Members.References.Request().AddAsync(requestBody);
            return true;
        }
        catch (Exception e)
        {

            throw;
        }
    }

    public async Task RemoveUserAsync(string userId)
    {
        try
        {
            await _graphServiceClient.Users[userId].Request().DeleteAsync();
        }
        catch (Exception e)
        {
            throw new GraphServiceException($"Could not create user in Azure Graph User. Ensure the MS Graph API Credentials are correct. UserId: {userId}, B2C TenantId: {_gsOptions.AzureB2CTenantId}," +
                $" AzureB2CGraphAccessAppRegClientId: {_gsOptions.AzureB2CGraphAccessAppRegClientId}", e);
        }
    }
}
