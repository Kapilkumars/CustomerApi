using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Module;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class ModuleMapperProfile: Profile
    {
        public ModuleMapperProfile()
        {
            CreateMap<ModuleModel, ModuleCosmosDb>()
                .ForMember(res => res.IsDeleted, pc => pc.MapFrom(x => false));
            CreateMap<ModuleCosmosDb, ModuleResponse>();

            CreateMap<ModuleResponse, ModuleCosmosDb>();
        }
    }
}
