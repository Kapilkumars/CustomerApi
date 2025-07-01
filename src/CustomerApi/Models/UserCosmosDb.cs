using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models;
[PartitionKeyPath("/id")]
public class UserCosmosDb : Item
{
    public string Type { get; set; }
    public string DisplayName { get; set; }
    public string CustomerNumber { get; set; }
    public string DefaultCustomerNumber { get; set; }
    public bool Admin { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<UserRoles> Roles { get; set; }
    public string Status { get; set; }
    public string GraphUserId { get; set; }
    public string GraphPrincipal { get; set; }
    public List<string> TenantsIds { get; set; } = new List<string>();

    public void SetGraphInfo(string graphUserId, string graphPrincipal) 
    { 
        GraphUserId = graphUserId;
        GraphPrincipal = graphPrincipal;
    }

    public void SetData(string type, string status)
    {
        Type = type;
        Status = status;
    }
    public void SetData(string displayName, string firstName, string lastName , string email, string status, List<string> tenantsIds, string defaultCustomerNumber, bool? admin)
    {
        DisplayName = displayName;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Status = status;
        TenantsIds = tenantsIds;
        DefaultCustomerNumber = defaultCustomerNumber;
        if (admin.HasValue)
        {
            Admin = admin.Value;
        }
    }


    public void SetRoles(List<UserRoles> roles)
    {
       Roles = roles;   
    }

}

public class UserRoles
{
    public string CustomerNumber { get; set; }
    public List<RoleCosmosDb> Roles { get; set; }
}
