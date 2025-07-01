using CustomerCustomerApi.Models.User;

namespace CustomerCustomerApi.Interfaces;

public interface IUserSvc
{
    Task<MetisUserResponse> CreateUserAsync(MetisUser userRequest, CancellationToken cancellationToken);
    Task<MetisUserResponse?> GetUserAsync(CancellationToken cancellationToken);
    Task<MetisUserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task<List<MetisUserResponse>> GetUserByGraphUserIdAsync(string graphUserId);
    Task RemoveUserAsync(string userId, CancellationToken cancellationToken);
    Task<List<MetisUserResponse>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<List<MetisUserResponse>> GetUsersByCustomerIdAsync(string customerId, CancellationToken cancellationToken);
    Task<bool> GetUserB2CRoles();
    Task<MetisUserResponse> UpdateAsync(MetisUser userRequest, string userId, bool updateAdmin, CancellationToken cancellationToken);
}
