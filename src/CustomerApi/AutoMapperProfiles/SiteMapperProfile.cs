using AutoMapper;
using Customer.Metis.Common.Models.Responses;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Site;

namespace CustomerCustomerApi.AutoMapperProfiles
{
    public class SiteMapperProfile : Profile
    {
        public SiteMapperProfile()
        {
            CreateMap<SiteModel, SiteCosmosDb>()
                .ForMember(s => s.Geometry, m => m.MapFrom(m => m.Geometry))
                .ForMember(s => s.ChildSpaces, m => m.MapFrom(m => m.ChildSpaces))
                .ForMember(s => s.Type, m => m.MapFrom(m => "SiteCosmosDb"));
            CreateMap<SiteCosmosDb, SiteModel>();

            CreateMap<SiteCosmosDb, SiteResponseModel>()
                .ForMember(s => s.Geometry, m => m.MapFrom(m => m.Geometry))
                .ForMember(s => s.ChildSpaces, m => m.MapFrom(m => m.ChildSpaces));

            CreateMap<SiteCosmosDb, SiteMetaDataResponse>();
            CreateMap<SiteResponseModel, SceneModel>();
            CreateMap<SiteCosmosDb, SiteGetModel>();
            //CreateMap<SiteModel, GeoJsonUpdateModel>();
        }
    }
}
