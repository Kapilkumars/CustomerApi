using CustomerCustomerApi.Models.Rbac;

namespace CustomerCustomerApi.Interfaces
{
    public interface IRoleSvc
    {
        Task<List<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken);
        Task<RoleResponse> CreateRoleAsync(RoleModel role, CancellationToken cancellationToken);
        Task<RoleResponse> UpdateRoleAsync(string roleId, RoleModel role, CancellationToken cancellationToken);
        Task RemoveRoleAsync(string roleId, CancellationToken cancellationToken); 
    }
}
