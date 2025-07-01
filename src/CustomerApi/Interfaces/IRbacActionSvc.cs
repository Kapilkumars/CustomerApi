using CommonModels.Enum;
using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.Interfaces
{
    public interface IRbacActionSvc
    {
        Task<List<RbacActionResponse>> GetActionsByCategoryAsync(ActionCategory? category, CancellationToken cancellationToken);
        Task<RbacActionResponse> CreateActionAsync(RbacActionModel action, CancellationToken cancellationToken);
        Task<RbacActionResponse> UpdateActionAsync(string actionId, RbacActionModel action, CancellationToken cancellationToken);
        Task RemoveActionAsync(string actionId, CancellationToken cancellationToken);
    }
}
