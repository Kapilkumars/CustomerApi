using CustomerCustomerApi.Models.Customer;

namespace CustomerCustomerApi.Interfaces
{
    public interface ICustomerSvc
    {
        Task<CustomerResponseModel> CreateCustomerAsync(CreateCustomerModel customerModel, CancellationToken cancellationToken);
        Task<CustomerResponseModel> UpdateCustomerAsync(string customerId, CustomerModel customerModel, CancellationToken cancellationToken);
        Task<CustomerResponseModel> GetCustomerByIdAsync(string customerId, CancellationToken cancellationToken);
        Task<List<CustomerResponseModel>> GetAllCustomersAsync(CancellationToken cancellationToken);
        Task<List<CustomerResponseModel>> GetCustomersByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task RemoveCustomerAsync(string customerId, CancellationToken cancellationToken);
        Task<bool> ExistCustomerAsync(string customerId);
        Task<CustomerResponseModel> AddTenantAsync(string customerId, TenantModel model, CancellationToken cancellationToken);
        Task DeleteTenantAsync(string customerId, string tenantId, CancellationToken cancellationToken);
        Task<CustomerResponseModel> UpdateTenantAsync(string customerId,  string tenantId, TenantModel model, CancellationToken cancellationToken);
        Task<bool> CanDeleteAsync(string customerId, CancellationToken cancellationToken);
    }
}