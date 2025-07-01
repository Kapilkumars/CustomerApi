using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Customer;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class CustomerMapperProfile : Profile
    {
        public CustomerMapperProfile()
        {
            CreateMap<CreateCustomerModel, CustomerCosmosDb>()
                .ForMember(dest => dest.CustomerAddress, act => act.MapFrom(act => act.CustomerAddress))
                .ForMember(dest => dest.Type, act => act.MapFrom(act => "Customer"))
                .ForMember(dest => dest.Tenants, act => act.Ignore());

            CreateMap<CustomerCustomerApi.Models.Customer.AddressModel, CustomerCustomerApi.Models.AddressModel>();

            CreateMap<CustomerCosmosDb, CustomerResponseModel>()
                .ForMember(dest => dest.CustomerAddress, act => act.MapFrom(act => act.CustomerAddress))
                .ForMember(dest => dest.Tenants, act => act.MapFrom(act => act.Tenants));

            CreateMap<CustomerCustomerApi.Models.AddressModel, CustomerCustomerApi.Models.Customer.AddressModel>();

            CreateMap<CustomerCustomerApi.Models.TenantModel, TenantResponseModel>()
                .ForMember(dest => dest.Subscriptions, act => act.MapFrom(act => act.Subscriptions));

            CreateMap<CustomerCustomerApi.Models.SubscriptionModel, SubscriptionResponseModel>()
               .ForMember(dest => dest.Entitlemets, act => act.MapFrom(act => act.Entitlemets));

            CreateMap<CustomerCustomerApi.Models.Entitlemet, EntitlemetResponseModel>()
               .ForMember(dest => dest.Module, act => act.MapFrom(act => act.Module));

            CreateMap<CustomerCustomerApi.Models.Customer.TenantModel, CustomerCustomerApi.Models.TenantModel>()
              .ForMember(dest => dest.SiteId, act => act.Ignore())
              .ForMember(dest => dest.LastModified, act => act.MapFrom(act => DateTime.UtcNow))
              .ForMember(dest => dest.Subscriptions, act => act.MapFrom(act => act.Subscriptions));

            CreateMap<CustomerCustomerApi.Models.Customer.SubscriptionModel, CustomerCustomerApi.Models.SubscriptionModel>()
               .ForMember(dest => dest.LastModified, act => act.MapFrom(act => DateTime.UtcNow))
               .ForMember(dest => dest.Entitlemets, act => act.MapFrom(act => act.Entitlemets));

            CreateMap<CustomerCustomerApi.Models.Customer.EntitlemetModel ,CustomerCustomerApi.Models.Entitlemet>()
               .ForMember(dest => dest.LastModified, act => act.MapFrom(act => DateTime.UtcNow))
               .ForMember(dest => dest.Module, act => act.MapFrom(act => act.Module));
        }
    }
}
