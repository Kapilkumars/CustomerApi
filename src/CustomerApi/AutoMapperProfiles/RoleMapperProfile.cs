using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class RoleMapperProfile : Profile
    {
        public RoleMapperProfile()
        {
            CreateMap<Properties, RoleProperties>();
            CreateMap<RoleProperties, Properties>();
            CreateMap<RoleCosmosDb, RoleResponse>()
                 .ForMember(dest => dest.Id, act => act.MapFrom(act => act.Id))
                 .ForMember(dest => dest.Name, act => act.MapFrom(act => act.Name))
                 .ForMember(dest => dest.Properties, act => act.MapFrom(act => act.Properties))
                 .ForMember(dest => dest.Permissions, act => act.MapFrom(act => act.Permissions));

            CreateMap<RolePermision, PermissionResponse>()
                .ForMember(dest => dest.UiActions, act => act.MapFrom(act => act.UiActions))
                .ForMember(dest => dest.DataActions, act => act.MapFrom(act => act.DataActions));

            CreateMap<CustomerCustomerApi.Models.ActionInfo, ActionInfoResponse>()
                 .ForMember(dest => dest.Resource, act => act.MapFrom(act => act.Resource))
                 .ForMember(dest => dest.Actions, act => act.MapFrom(act => act.Actions));

            CreateMap<RoleModel, RoleCosmosDb>()
                 .ForMember(dest => dest.Name, act => act.MapFrom(act => act.Name))
                 .ForMember(dest => dest.Properties, act => act.MapFrom(act => act.Properties))
                 .ForMember(dest => dest.Permissions, act => act.Ignore());
        }
    }
}
