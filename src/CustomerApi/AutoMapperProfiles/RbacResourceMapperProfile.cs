using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class RbacResourceMapperProfile : Profile
    {
        public RbacResourceMapperProfile()
        {
            CreateMap<RbacResourceCosmosDb, RbacResourceResponse>();
            CreateMap<RbacResourceModel, RbacResourceCosmosDb>()
                .ForMember(dest => dest.IsDeleted, res => res.MapFrom(x => false));
        }
    }
}
