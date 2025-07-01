using Customer.Metis.Common.Models.Responses;
using CustomerCustomerApi.Models.Site;
using CustomerCustomerApi.ResponseHelpers;

namespace CustomerCustomerApi.Interfaces;
public interface ISiteSvc
{
    Task<List<SiteResponseModel>> GetAllAsync(string? siteName = null); //only for backwards compatibility allow siteName
    Task<SiteResponseModel?> TryFindByIdAsync(string id);
    Task<SiteResponseModel> CreateSiteAsync(SiteModel model);
    Task<SiteResponseModel> UpdateSiteAsync(SiteModel model, string id);
    Task DeleteSiteAsync(string id);
    Task<bool> ExistAsync(string siteName);
    Task<bool> ExistAsync(string siteName, string floorName);
    Task<bool> SpaceWithFloorExistAsync(string siteId, string floorId);
    Task<SiteGetModel> GetSiteByIdAsync(string id);
    Task<SiteResponseModel> GetSpaceBySiteIdAndSpaceAsync(string siteId, string space);
    Task<SiteModel> GetSpaceBySiteIdAndSpaceAsyncNew(string siteId, string space);
    Task<List<SiteMetaDataResponse>> GetMetaDataAsync();
    List<SceneModel> GetScenesBySiteIdFloorAndSpace(string siteId, string? floor = null, string? space = null);
    List<SceneModel> GetGroupsBySiteIdFloorAndSpace(string siteId, string? floor = null, string? space = null);
    Task<string?> RevertSiteAsync(string siteId, string floorId);
    Task UpdateFileAsync(string siteId, string floorId, FileUploadRequest model);
}