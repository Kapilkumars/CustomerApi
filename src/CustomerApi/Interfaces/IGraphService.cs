using CommonModels;
using CustomerCustomerApi.Models.User;
using Microsoft.Graph;

namespace CustomerCustomerApi.Interfaces;

public interface IGraphService
{
     Task<User> CreateUserAsync(MetisUser user);
    Task RemoveUserAsync(string userId);
    Task<bool> GetB2CUserRoles();
}
