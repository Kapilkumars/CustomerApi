using CustomerCustomerApi.Models.Module;
using CustomerCustomerApi.Models.User;

namespace CustomerCustomerApi.Models.Customer
{
    public class CustomerModel
    {
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public AddressModel CustomerAddress { get; set; }
        public List<TenantModel>? Tenants { get; set; }
        public string Type { get; set; }
    }
    public class AddressModel
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Culture { get; set; }
    }

    public class TenantModel
    {
        public string TenantName { get; set; }
        public string TenantStatus { get; set; }
        public string InvitationEmail { get; set; }
        public string? SiteId { get; set; }
        public List<SubscriptionModel>? Subscriptions { get; set; }
    }

    public class SubscriptionModel
    {
        public string SubscriptionNumber { get; set; }
        public int SubscriptionCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<EntitlemetModel>? Entitlemets { get; set; }
    }

    public class EntitlemetModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IsActive { get; set; }
        public int EntitlemetCount { get; set; }
        public int GracePreiodDays { get; set; }
        public ModuleResponse? Module { get; set; }
    }

    public class CustomerResponseModel
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public AddressModel CustomerAddress { get; set; }
        public List<TenantResponseModel>? Tenants { get; set; }
        public string Type { get; set; }

        //merging with CustomerInfoResponse - needs cleanup
        public List<string> Sites { get; set; }
    }

    public class CustomerWithUsersResponse
    {
        public List<MetisUserResponse> Users { get; set; }
        public CustomerResponseModel Customer { get; set; }
    }

    public class AllCustomerWithAllUsersResponse
    {
        public List<MetisUserResponse> Users { get; set; }
        public List<CustomerResponseModel> Customers { get; set; }
    }

    public class TenantResponseModel
    {
        public string Id { get; set; }
        public string TenantName { get; set; }
        public string TenantStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public string InvitationEmail { get; set; }
        public string SiteId { get; set; }
        public List<SubscriptionResponseModel>? Subscriptions { get; set; }
    }

    public class SubscriptionResponseModel
    {
        public string SubscriptionNumber { get; set; }
        public int SubscriptionCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public List<EntitlemetResponseModel>? Entitlemets { get; set; }
    }

    public class EntitlemetResponseModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IsActive { get; set; }
        public DateTime CancelledDate { get; set; }
        public DateTime LastModified { get; set; }
        public int EntitlemetCount { get; set; }
        public int GracePreiodDays { get; set; }
        public ModuleResponse? Module { get; set; }
    }
}
