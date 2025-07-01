using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.User;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using User = Microsoft.Graph.User;

namespace CustomerCustomerApi.Services;

public class UserSvc : IUserSvc
{
    private readonly IGraphService _graphService;
    private readonly IRepository<UserCosmosDb> _userCosmosRepository;
    private readonly IRepository<RoleCosmosDb> _roleCosmosRepository;
    private readonly IEmailSvc _emailSvc;
    private readonly IMapper _mapper;
    private readonly IRepository<CustomerCosmosDb> _customerCosmosRepository;
    private readonly ICustomerSvc _customerSvc;
    private readonly IAuthToUserProvider _authToUserProvider;

    public UserSvc(IGraphService graphService, 
                   IRepository<UserCosmosDb> userCosmosRepository, 
                   IRepository<RoleCosmosDb> roleCosmosRepository, 
                   IEmailSvc emailSvc, 
                   IMapper mapper,
                   IRepository<CustomerCosmosDb> customerCosmosRepository,
                   ICustomerSvc customerSvc,
                   IAuthToUserProvider authToUserProvider)
    {
        _graphService = graphService;
        _userCosmosRepository = userCosmosRepository;
        _roleCosmosRepository = roleCosmosRepository;
        _emailSvc = emailSvc;
        _mapper = mapper;
        _customerCosmosRepository = customerCosmosRepository;
        _customerSvc = customerSvc;
        _authToUserProvider = authToUserProvider;
    }

    public async Task<MetisUserResponse> CreateUserAsync(MetisUser userRequest, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _customerSvc.ExistCustomerAsync(userRequest.DefaultCustomerNumber) || !await _customerSvc.ExistCustomerAsync(userRequest.CustomerNumber))
                throw new UserSvcException($"Not found the default customer. Customer number: {userRequest.DefaultCustomerNumber}", System.Net.HttpStatusCode.NotFound);

            User graphUser = await _graphService.CreateUserAsync(userRequest);
            var userRoles = new List<UserRoles>();

            foreach (var role in userRequest.Roles)
            {
                var roles = await _roleCosmosRepository.GetAsync(x => role.Roles.Contains(x.Id), cancellationToken);
                userRoles.Add(new UserRoles { CustomerNumber = role.CustomerNumber, Roles = roles.ToList()});
            }

            var userItem = _mapper.Map<UserCosmosDb>(userRequest);
            userItem.SetRoles(userRoles);
            userItem.SetGraphInfo(graphUser.Id, graphUser.UserPrincipalName);
            userItem.SetData("User", "created");

            var cosmosUser = await _userCosmosRepository.CreateAsync(userItem);
            var emailResponse = await _emailSvc.SendUserEmailAsync(userRequest.Email,
                                                    userRequest.FirstName,
                                                    graphUser.PasswordProfile.Password,
                                                    String.Join(",", userRequest.Roles));

            return _mapper.Map<MetisUserResponse>(cosmosUser);
        }
        catch (GraphServiceException ge)
        {
            throw new UserSvcException($"Could not create graph user in AAD. Look at the inner exception. Email: {userRequest.Email}", ge);
        }
        catch (CosmosException ex)
        {
            var exception = new UserSvcException($"Could not create Metis User. Look at the inner exception. Email: {userRequest.Email}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
            throw exception;
        }
        catch (UserSvcException ex)
        {
            throw new UserSvcException($"Could not create Metis User. Look at the inner exception. Email: {userRequest.Email}", ex.HttpStatusCode);
        }
        catch (Exception e)
        {
            throw new UserSvcException($"Could not create Metis User. Look at the inner exception. Email: {userRequest.Email}", e);
        }
    }

    public async Task<MetisUserResponse?> GetUserAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userItems = await _userCosmosRepository.GetAsync(x => x.GraphUserId == _authToUserProvider.GraphUserId, cancellationToken);

            if (!userItems.Any())
                throw new UserSvcException($"User with graphUserId : {_authToUserProvider.GraphUserId} - not found", System.Net.HttpStatusCode.NotFound);

            var result = _mapper.Map<MetisUserInfoResponse>(userItems.First());

            return result;
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException("Could not obtain Metis User. Look at the inner exception.", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not obtain Metis User. Look at the inner exception.", ex);
        }
    }

    public async Task<List<MetisUserResponse>> GetUserByGraphUserIdAsync(string graphUserId)
    {
        try
        {
            var userItems = await _userCosmosRepository.GetAsync(x => x.GraphUserId == graphUserId);

            if (!userItems.Any())
                throw new UserSvcException($"User with graphUserId : {graphUserId} - not found", System.Net.HttpStatusCode.NotFound);

            var result = _mapper.Map<List<MetisUserResponse>>(userItems);

            return result;
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException($"Could not obtain Metis User using the graphUserId. graphUserId: {graphUserId}.", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not obtain Metis User using the graphUserId. graphUserId: {graphUserId}.", ex);
        }
    }

    public async Task<MetisUserResponse?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var userItems = await _userCosmosRepository.GetAsync(x => x.Id == userId, cancellationToken);
            if (!userItems.Any())
                throw new UserSvcException($"User with userId : {userId} - not found", System.Net.HttpStatusCode.NotFound);

            var result = _mapper.Map<MetisUserResponse>(userItems.First());

            return result;
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException("Could not obtain Metis User. Look at the inner exception.", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (UserSvcException ex)
        {
            throw new UserSvcException($"Could not obtain Metis User. Look at the inner exception. {ex.Message}", ex.HttpStatusCode);
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not obtain Metis User. Look at the inner exception.", ex);
        }
    }

    public async Task<List<MetisUserResponse>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userItems = await _userCosmosRepository.GetAsync(x => true, cancellationToken);
            return _mapper.Map<List<MetisUserResponse>>(userItems);
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException($"Could not obtain Metis Users. Look at the inner exception.", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (UserSvcException ex)
        {
            throw new UserSvcException($"Could not obtain Metis User. Look at the inner exception. {ex.Message}", ex.HttpStatusCode);
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not obtain Metis Users. Look at the inner exception.", ex);
        }
    }

    public async Task<List<MetisUserResponse>> GetUsersByCustomerIdAsync(string customerId, CancellationToken cancellationToken)
    {
        try
        {
            //filter users by customerId
            var customer = (await _customerCosmosRepository.GetAsync(x => x.Id == customerId, cancellationToken)).FirstOrDefault();
            var customerNumber = customer?.CustomerNumber;

            if (string.IsNullOrEmpty(customerNumber))
                throw new UserSvcException($"Customer with Id : {customerId} - not found", System.Net.HttpStatusCode.NotFound);

            var userItems = await _userCosmosRepository.GetAsync(x => x.Roles.Any(r => r.CustomerNumber == customerNumber), cancellationToken);
            return _mapper.Map<List<MetisUserResponse>>(userItems);
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException($"Could not obtain Metis Users. Look at the inner exception. Customer id: {customerId}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (UserSvcException ex)
        {
            throw new UserSvcException($"Could not obtain Metis User. Look at the inner exception. {ex.Message}", ex.HttpStatusCode);
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not obtain Metis Users. Look at the inner exception. Customer id: {customerId}", ex);
        }
    }


    public async Task RemoveUserAsync(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = (await _userCosmosRepository.GetAsync(x => x.Id == userId, cancellationToken)).FirstOrDefault();
            if (user == default)
                throw new UserSvcException($"User with Id : {userId} - not found", System.Net.HttpStatusCode.NotFound);
            if (user != null)
            {
                await _graphService.RemoveUserAsync(user.GraphUserId);
                await _userCosmosRepository.DeleteAsync(user.Id, user.CustomerNumber);
                //Todo: send email to the user that he has been removed.
            }
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException($"Could not delete Metis Users. Look at the inner exception. User id: {userId}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (UserSvcException ex)
        {
            throw new UserSvcException($"Could not obtain Metis User. Look at the inner exception. {ex.Message}", ex.HttpStatusCode);
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not delete Metis Users. Look at the inner exception. User id: {userId}", ex);
        }
    }

    public async Task<MetisUserResponse> UpdateAsync(MetisUser userRequest, string userId, bool updateAdmin, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _customerSvc.ExistCustomerAsync(userRequest.DefaultCustomerNumber))
                throw new UserSvcException($"Not found the default customer. Customer number: {userRequest.DefaultCustomerNumber}", System.Net.HttpStatusCode.NotFound);

            var dbUserResult = await _userCosmosRepository.GetAsync(x => x.Id == userId, cancellationToken);
            var dbUser = dbUserResult.FirstOrDefault();

            if (dbUser == default)
                throw new UserSvcException($"User not found. User id : {_authToUserProvider.GraphUserId}", System.Net.HttpStatusCode.NotFound);
            var userRoles = new List<UserRoles>();

            foreach (var role in userRequest.Roles)
            {
                var roles = await _roleCosmosRepository.GetAsync(x => role.Roles.Contains(x.Id), cancellationToken);
                userRoles.Add(new UserRoles { CustomerNumber = role.CustomerNumber, Roles = roles.ToList() });
            }
            dbUser.SetData(userRequest.DisplayName,
                userRequest.FirstName,
                userRequest.LastName,
                userRequest.Email,
                userRequest.Status,
                userRequest.TenantsIds,
                userRequest.DefaultCustomerNumber,
                updateAdmin ? userRequest.Admin : null);
            dbUser.SetRoles(userRoles);

            var updatedCustomer = await _userCosmosRepository.UpdateAsync(dbUser);

            return _mapper.Map<MetisUserResponse>(updatedCustomer);
        }
        catch (CosmosException ex)
        {
            throw new UserSvcException($"Could not update Metis Users. Look at the inner exception. Graph user id: {_authToUserProvider.GraphUserId}", ex)
            {
                HttpStatusCode = ex.StatusCode
            };
        }
        catch (UserSvcException ex)
        {
            throw new UserSvcException($"Could not create Metis User. {ex.Message}", ex.HttpStatusCode);
        }
        catch (Exception ex)
        {
            throw new UserSvcException($"Could not update Metis Users. Look at the inner exception. Graph user id: {_authToUserProvider.GraphUserId}", ex);
        }
    }
    public async Task<bool> GetUserB2CRoles()
    {
        return await _graphService.GetB2CUserRoles();
    }
}
