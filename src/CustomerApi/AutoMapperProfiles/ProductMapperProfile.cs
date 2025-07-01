using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Product;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class ProductMapperProfile : Profile
    {
        public ProductMapperProfile()
        {
            CreateMap<ProductCosmosDb, ProductResponse>()
             .ForMember(res => res.Skus, pc => pc.MapFrom(pc => pc.Skus));
            CreateMap<ProductModel, ProductCosmosDb>()
                 .ForMember(res => res.Skus, pc => pc.Ignore());

            CreateMap<Skus, SkusResponse>()
                .ForMember(s => s.Modules, sr => sr.MapFrom(sr => sr.Modules));
        }
    }
}
