using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.User;

namespace CustomerCustomerApi.AutoMapperProfiles;

public class MetisUserProfile : Profile
{
    public MetisUserProfile()
    {
        CreateMap<UserCosmosDb, MetisUser>().ForMember(dest => dest.Roles, act => act.Ignore());
        CreateMap<MetisUser, UserCosmosDb>().ForMember(dest => dest.Roles, act => act.Ignore());
        CreateMap<MetisUser, MetisUserResponse>().ForMember(dest => dest.Roles, act => act.Ignore());
        CreateMap<MetisUserResponse, MetisUser>().ForMember(dest => dest.Roles, act => act.Ignore());

        CreateMap<UserCosmosDb, MetisUserResponse>()
            .ForMember(dest => dest.DefaultCustomerNumber, act => act.MapFrom(act => string.IsNullOrEmpty(act.DefaultCustomerNumber) ? act.CustomerNumber : act.DefaultCustomerNumber))
            .ForMember(dest => dest.Roles, act => act.MapFrom(act => act.Roles));

        CreateMap<MetisUserResponse, UserCosmosDb>().ForMember(dest => dest.Roles, act => act.Ignore());

        CreateMap<UserCosmosDb, MetisUserInfoResponse>()
            .ForMember(dest => dest.DefaultCustomerNumber, act => act.MapFrom(act => string.IsNullOrEmpty(act.DefaultCustomerNumber) ? act.CustomerNumber : act.DefaultCustomerNumber))
            .ForMember(dest => dest.Roles, act => act.MapFrom(act => act.Roles));

        CreateMap<UserRoles, UserRolesResponse>().ForMember(dest => dest.Roles, act => act.MapFrom(act => act.Roles));
        //CreateMap<CommonModels.Properties, PropertiesCosmosDb>();
        //CreateMap<PropertiesCosmosDb, CommonModels.Properties>();
        //CreateMap<PermissionCosmosDb, Permission>();
        //CreateMap<Permission, PermissionCosmosDb>();
        //CreateMap<List<PermissionCosmosDb>, List<Permission>>();
        //CreateMap<List<Permission>, List<PermissionCosmosDb>>();

    }
}
