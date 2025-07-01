using CustomerCustomerApi.Models.Customer;
using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.Models.User;
public class MetisUser
{
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CustomerNumber { get; set; }
    public string DefaultCustomerNumber { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public List<MetisUserRoles> Roles { get; set; }
    public List<string> TenantsIds { get; set; }
    public bool Admin { get; set; }
}

public class MetisUserRoles
{
    public string CustomerNumber { get; set; }
    public List<string> Roles { get; set; }
}
public class MetisUserResponse
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CustomerNumber { get; set; }
    public string DefaultCustomerNumber { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public List<UserRolesResponse> Roles { get; set; }
    public List<string> TenantsIds { get; set; }
    public bool Admin { get; set; }
    public string GraphUserId { get; set; }
    public string GraphPrincipal { get; set; }
}

public class UserRolesResponse
{
    public string CustomerNumber { get; set; }
    public List<RoleResponse> Roles { get; set; }
}


public class MetisUserInfoResponse : MetisUserResponse
{
    public List<CustomerResponseModel> Customers { get; set; }
}