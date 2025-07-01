using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;

namespace CustomerCustomerApi.Services
{
    public class RbacResourceSvc : IRbacResourceSvc
    {
        private readonly IMapper _mapper;
        private readonly IRepository<RbacResourceCosmosDb> _resourceCosmosRepository;

        public RbacResourceSvc(IMapper mapper, 
                               IRepository<RbacResourceCosmosDb> resourceCosmosRepository)
        {
            _mapper = mapper;
            _resourceCosmosRepository = resourceCosmosRepository;
        }

        public async Task<RbacResourceResponse> CreateResourceAsync(RbacResourceModel resource, CancellationToken cancellationToken)
        {
            try
            {
                var resourceItem = _mapper.Map<RbacResourceCosmosDb>(resource);
                var result = await _resourceCosmosRepository.CreateAsync(resourceItem, cancellationToken);

                return _mapper.Map<RbacResourceResponse>(result);
            }
            catch (CosmosException ex)
            {
                throw new RBACException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (RBACException ex)
            {
                throw new RBACException("Error during creating RBAC resource, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not create RBAC resource. Look at the inner exception.", ex);
            }
        }

        public async Task<List<RbacResourceResponse>> GetAllResourcesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var resourceItems = await _resourceCosmosRepository.GetAsync(x => true, cancellationToken);
                return _mapper.Map<List<RbacResourceResponse>>(resourceItems);
            }
            catch (CosmosException ex)
            {
                throw new RBACException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not create RBAC resource. Look at the inner exception.", ex);
            }
        }

        public async Task<RbacResourceResponse> UpdateResourceAsync(string resourceId, RbacResourceModel resource, CancellationToken cancellationToken)
        {
            try
            {
                var resourceItems = await _resourceCosmosRepository.GetAsync(x => x.Id == resourceId, cancellationToken);
                if (!resourceItems.Any())
                    throw new NotFoundExeption($"Could not find resource. Look at the inner exception. Resource id: {resourceId}");

                var resourceItem = resourceItems.First();
                resourceItem.Update(resource.Description);

                await _resourceCosmosRepository.UpdateAsync(resourceItem, cancellationToken: cancellationToken);

                return _mapper.Map<RbacResourceResponse>(resourceItem);
            }
            catch (CosmosException ex)
            {
                throw new RBACException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (RBACException ex)
            {
                throw new RBACException("Error during updating RBAC resource, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not updating RBAC resource. Look at the inner exception.", ex);
            }
        }

        public async Task RemoveResourceAsync(string resourceId, CancellationToken cancellationToken)
        {
            try
            {
                var resourceItems = await _resourceCosmosRepository.GetAsync(x => x.Id == resourceId, cancellationToken);
                if (!resourceItems.Any())
                    throw new NotFoundExeption($"Could not find resource. Look at the inner exception. Resource id: {resourceId}");

                var resourceItem = resourceItems.First();
                resourceItem.ToggleIsDeleted();
                await _resourceCosmosRepository.UpdateAsync(resourceItem, cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                throw new RBACException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (RBACException ex)
            {
                throw new RBACException("Error during deleting RBAC resource, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException($"Could not deleting RBAC resource. Look at the inner exception.", ex);
            }
        }
    }
}
