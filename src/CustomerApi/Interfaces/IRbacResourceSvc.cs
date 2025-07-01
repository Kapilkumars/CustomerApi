using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.Interfaces
{
    public interface IRbacResourceSvc
    {
        Task<List<RbacResourceResponse>> GetAllResourcesAsync(CancellationToken cancellationToken);
        Task<RbacResourceResponse> CreateResourceAsync(RbacResourceModel resource, CancellationToken cancellationToken);
        Task<RbacResourceResponse> UpdateResourceAsync(string resourceId, RbacResourceModel resource, CancellationToken cancellationToken);
        Task RemoveResourceAsync(string resourceId, CancellationToken cancellationToken);
    }
}
