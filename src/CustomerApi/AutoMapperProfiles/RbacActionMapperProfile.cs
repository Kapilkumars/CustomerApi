using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class RbacActionMapperProfile : Profile
    {
        public RbacActionMapperProfile()
        {
            CreateMap<RbacActionCosmosDb, RbacActionResponse>();
            CreateMap<RbacActionModel, RbacActionCosmosDb>()
                .ForMember(dest => dest.IsDeleted, act => act.MapFrom(x => false))
                .ForMember(dest => dest.Category, act => act.MapFrom(act => act.Category.ToString()));
        }
    }
}
