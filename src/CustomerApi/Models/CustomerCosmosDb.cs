using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.CosmosRepository.Attributes;

namespace CustomerCustomerApi.Models
{
    [PartitionKeyPath("/id")]
    public class CustomerCosmosDb : Item
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public AddressModel CustomerAddress { get; set; } = new AddressModel();
        public List<TenantModel>? Tenants { get; set; } = new List<TenantModel>();
        public string Type { get; set; }

        public void Update(List<TenantModel> tenants)
        {
            Tenants = tenants;
        }
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
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string TenantName { get; set; }
        public string TenantStatus { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; }
        public string InvitationEmail { get; set; }
        public string SiteId { get; set; }
        public List<SubscriptionModel>? Subscriptions { get; set; }

        public void Update(string tenantName, string tenantStatus, string invitationEmail, string siteId, List<SubscriptionModel>? subscriptions)
        {
            TenantName = tenantName;
            TenantStatus = tenantStatus;
            InvitationEmail = invitationEmail;
            SiteId = siteId;
            LastModified = DateTime.UtcNow;
 
            if (subscriptions != null && subscriptions.Any() && Subscriptions != null)
            {
                foreach (var sub in Subscriptions)
                {
                    if (subscriptions.Any(x => x.SubscriptionNumber == sub.SubscriptionNumber))
                    {
                        sub.Update(subscriptions.First(x => x.SubscriptionNumber == sub.SubscriptionNumber));
                    }
                }
            }

            if (Subscriptions is null && subscriptions != null && subscriptions.Any())
            {
                Subscriptions = subscriptions;
            }
        }
    }

    public class SubscriptionModel
    {
        public void Update(SubscriptionModel subscription)
        {
            SubscriptionCount = subscription.SubscriptionCount;
            StartDate = subscription.StartDate;
            EndDate = subscription.EndDate;
            CancelledDate = subscription.CancelledDate;
            LastModified = DateTime.UtcNow;
            Entitlemets = subscription.Entitlemets;
        }

        public string SubscriptionNumber { get; set; }
        public int SubscriptionCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CancelledDate { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; }
        public List<Entitlemet>? Entitlemets { get; set; }
    }

    public class Entitlemet
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IsActive { get; set; }
        public DateTime CancelledDate { get; set; }
        public DateTime LastModified { get; set; }
        public int EntitlemetCount { get; set; }
        public int GracePreiodDays { get; set; }
        public ModuleCosmosDb? Module { get; set; }
    }
}
