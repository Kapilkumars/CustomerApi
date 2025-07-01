using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Customer;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using System.Data;

namespace CustomerCustomerApi.Services
{
    public class CustomerSvc : ICustomerSvc
    {
        private readonly IMapper _mapper;
        private readonly IRepository<CustomerCosmosDb> _customerCosmosRepository;
        private readonly IRepository<UserCosmosDb> _userCosmosRepository;
        private readonly IRepository<SiteCosmosDb> _siteCosmosRepository;
        private readonly IAuthToUserProvider _authToUserProvider;

        public CustomerSvc(IMapper mapper,
                           IRepository<CustomerCosmosDb> customerCosmosRepository,
                           IRepository<UserCosmosDb> userCosmosRepository,
                           IRepository<SiteCosmosDb> siteCosmosRepository,
                           IAuthToUserProvider authToUserProvider)
        {
            _mapper = mapper;
            _customerCosmosRepository = customerCosmosRepository;
            _userCosmosRepository = userCosmosRepository;
            _siteCosmosRepository = siteCosmosRepository;
            _authToUserProvider = authToUserProvider;
        }

        public async Task<CustomerResponseModel> CreateCustomerAsync(CreateCustomerModel customerModel, CancellationToken cancellationToken)
        {
            try
            {
                var customerItem = _mapper.Map<CustomerCosmosDb>(customerModel);
                var createdCustomer = await _customerCosmosRepository.CreateAsync(customerItem, cancellationToken);

                return _mapper.Map<CustomerResponseModel>(createdCustomer);
            }
            catch (CustomerSvcException ex)
            {
                throw new CustomerSvcException("Can't create the customer.", System.Net.HttpStatusCode.BadRequest, ex);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException("Can't create the customer in database.", System.Net.HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException("An unexpected error occurred while creating the customer.", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<CustomerResponseModel> UpdateCustomerAsync(string customerId, CustomerModel customerModel, CancellationToken cancellationToken)
        {
            try
            {
                var customerItem = await _customerCosmosRepository.GetAsync(customerId, cancellationToken: cancellationToken);
                if (customerItem is null)
                    throw new CustomerSvcException($"Not found the customer. Customer id {customerId}", System.Net.HttpStatusCode.NotFound);

                if (customerModel.Tenants != null)
                {
                    foreach (var tenantModel in customerModel.Tenants.Where(x => !string.IsNullOrEmpty(x.SiteId)))
                    {
                        if (!await ExistSiteAsync(tenantModel.SiteId!))
                            throw new CustomerSvcException($"Can't update the customer. One of the site not exist.", System.Net.HttpStatusCode.BadRequest);
                    }
                }

                var tenents = _mapper.Map<List<Models.TenantModel>>(customerModel.Tenants);

                customerItem.CustomerName = customerModel.CustomerName;
                customerItem.CustomerAddress = _mapper.Map<Models.AddressModel>(customerModel.CustomerAddress);
                customerItem.Update(tenents);
                var updatedCustomer = await _customerCosmosRepository.UpdateAsync(customerItem, cancellationToken: cancellationToken);

                return _mapper.Map<CustomerResponseModel>(updatedCustomer);
            }
            catch (CustomerSvcException ex)
            {
                throw new CustomerSvcException(ex.Message, ex.HttpStatusCode, ex);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't update the customer in database. Customer id {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while updating the customer. Customer id {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<CustomerResponseModel> GetCustomerByIdAsync(string customerId, CancellationToken cancellationToken)
        {
            try
            {
                var customerItems = await _customerCosmosRepository.GetAsync(x => x.Id == customerId, cancellationToken);

                if (!customerItems.Any())
                    throw new NotFoundExeption("An unexpected error occurred while obtaining customers.");

                return _mapper.Map<CustomerResponseModel>(customerItems.First());
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Unable to get customers from database with customerNumber: {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException("An unexpected error occurred while obtaining customers.", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<List<CustomerResponseModel>> GetAllCustomersAsync(CancellationToken cancellationToken)
        {
            try
            {
                var customerItems = await _customerCosmosRepository.GetAsync(x => true, cancellationToken);
                return _mapper.Map<List<CustomerResponseModel>>(customerItems);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Unable to get customers from database.", System.Net.HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException("An unexpected error occurred while obtaining customers.", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task RemoveCustomerAsync(string customerId, CancellationToken cancellationToken)
        {
            try
            {
                var customers = await _customerCosmosRepository.GetAsync(x => x.Id == customerId);
                if (!customers.Any())
                {
                    throw new CustomerSvcException("Customer not found.")
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
                var customer = customers.First();

                await _customerCosmosRepository.DeleteAsync(customer, cancellationToken);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't delete the customer in database. Customer id {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer id {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<bool> ExistCustomerAsync(string customerNumber)
        {
            try
            {
                var items = await _customerCosmosRepository.GetAsync(x => x.CustomerNumber == customerNumber);
                return items.Any();
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't find the customer in database. Customer number : {customerNumber}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer number : {customerNumber}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<CustomerResponseModel> AddTenantAsync(string customerId, Models.Customer.TenantModel model, CancellationToken cancellationToken)
        {
            try
            {
                var customers = await _customerCosmosRepository.GetAsync(x => x.Id == customerId);
                if (!customers.Any())
                {
                    throw new CustomerSvcException("Customer not found.")
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
                var customer = customers.First();

                var tenant = _mapper.Map<Models.TenantModel>(model);
                if (await ExistSiteAsync(model.SiteId))
                {
                    tenant.SiteId = model.SiteId;
                }

                if (customer.Tenants is null)
                {
                    customer.Tenants = new List<Models.TenantModel>();
                }

                customer.Tenants.Add(tenant);

                var newCustomer = await _customerCosmosRepository.UpdateAsync(customer, cancellationToken: cancellationToken);

                var users = await _userCosmosRepository.GetAsync(x => (x.CustomerNumber == customer.CustomerNumber || x.Roles
                .Any(r => r.CustomerNumber == customer.CustomerNumber)), cancellationToken);

                foreach (var user in users)
                {
                    if (!user.TenantsIds.Any(x => x == tenant.id))
                    {
                        user.TenantsIds.Add(tenant.id);
                    }
                }

                await _userCosmosRepository.UpdateAsync(users, cancellationToken: cancellationToken);
                return _mapper.Map<CustomerResponseModel>(customer);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't find the customer in database. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer number : {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<CustomerResponseModel> UpdateTenantAsync(string customerId, string tenantId, Models.Customer.TenantModel model, CancellationToken cancellationToken)
        {
            try
            {
                var customers = await _customerCosmosRepository.GetAsync(x => x.Id == customerId);
                if (!customers.Any())
                {
                    throw new CustomerSvcException("Customer not found.")
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
                var customer = customers.First();

                if (customer.Tenants == null || !customer.Tenants.Any(x => x.id == tenantId))
                {
                    throw new CustomerSvcException("Tenant not found.", System.Net.HttpStatusCode.NotFound);
                }

                if (!string.IsNullOrEmpty(model.SiteId))
                {
                    if (!await ExistSiteAsync(model.SiteId!))
                    {
                        throw new CustomerSvcException($"Can't update the customer. Site not exist.", System.Net.HttpStatusCode.BadRequest);
                    }
                }

                foreach (var tenant in customer.Tenants)
                {
                    if (tenant.id == tenantId)
                    {
                        tenant.Update(
                            model.TenantName,
                            model.TenantStatus,
                            model.InvitationEmail,
                            model.SiteId,
                            _mapper.Map<List<Models.SubscriptionModel>>(model.Subscriptions));
                    }
                }

                var newCustomer = await _customerCosmosRepository.UpdateAsync(customer, cancellationToken: cancellationToken);

                return _mapper.Map<CustomerResponseModel>(customer);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't find the customer in database. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (CustomerSvcException ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer id : {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task DeleteTenantAsync(string customerId, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                var customers = await _customerCosmosRepository.GetAsync(x => x.Id == customerId);
                if (!customers.Any())
                {
                    throw new CustomerSvcException("Customer not found.")
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
                var customer = customers.First();

                if (customer.Tenants == null || !customer.Tenants.Any(x => x.id == tenantId))
                {
                    throw new CustomerSvcException("Tenant not found.")
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                customer.Tenants = customer.Tenants.Where(x => x.id != tenantId).ToList();

                var newCustomer = await _customerCosmosRepository.UpdateAsync(customer, cancellationToken: cancellationToken);

                var users = await _userCosmosRepository.GetAsync(x => (x.CustomerNumber == customer.CustomerNumber || x.Roles
                .Any(r => r.CustomerNumber == customer.CustomerNumber)), cancellationToken);

                foreach (var user in users)
                {
                    user.TenantsIds = user.TenantsIds.Where(x => x != tenantId).ToList();
                }

                await _userCosmosRepository.UpdateAsync(users, cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't find the customer in database. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (CustomerSvcException ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer od : {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        public async Task<bool> CanDeleteAsync(string customerId, CancellationToken cancellationToken)
        {
            try
            {
                var customers = await _customerCosmosRepository.GetAsync(x => x.Id == customerId);
                if (!customers.Any())
                {
                    throw new CustomerSvcException("Customer not found.")
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
                var customer = customers.First();

                var users = await _userCosmosRepository.GetAsync(x =>
                                        (x.CustomerNumber == customer.CustomerNumber || 
                                        x.Roles.Any(r => r.CustomerNumber == customer.CustomerNumber)), cancellationToken);

                return !users.Any();
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Can't find the customer in database. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (CustomerSvcException ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer id : {customerId}", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while deleting the customer. Customer id : {customerId}", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }

        private async Task<bool> ExistSiteAsync(string siteId)
        {
            return await _siteCosmosRepository.ExistsAsync(x => x.Id == siteId);
        }

        public async Task<List<CustomerResponseModel>> GetCustomersByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                var userItems = await _userCosmosRepository.GetAsync(x => x.Id == userId, cancellationToken);
                if (!userItems.Any())
                    throw new CustomerSvcException($"User with graphUserId : {_authToUserProvider.GraphUserId} - not found", System.Net.HttpStatusCode.NotFound);

                var customersInfo = new List<CustomerResponseModel>();
                var user = userItems.First();
                foreach (var role in user.Roles)
                {
                    var customers = await _customerCosmosRepository.GetAsync(x => x.CustomerNumber == role.CustomerNumber, cancellationToken);
                    var customer = customers.First();
                    customersInfo.Add(new CustomerResponseModel
                    {
                        Id = customer.Id,
                        CustomerName = customer.CustomerName,
                        CustomerNumber = customer.CustomerNumber,
                        Sites = customer.Tenants.Select(x => x.SiteId).Where(s => !string.IsNullOrEmpty(s)).ToList(),
                        Tenants = _mapper.Map<List<TenantResponseModel>>(customer.Tenants)
                    });
                }

                return customersInfo;
            }
            catch (CosmosException ex)
            {
                throw new CustomerSvcException($"Unable to get customers from database", System.Net.HttpStatusCode.InternalServerError, ex);
            }
            catch (CustomerSvcException ex)
            {
                throw new CustomerSvcException($"An unexpected error occurred while obtaining data from db.", System.Net.HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                throw new CustomerSvcException("An unexpected error occurred while obtaining customers.", System.Net.HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}