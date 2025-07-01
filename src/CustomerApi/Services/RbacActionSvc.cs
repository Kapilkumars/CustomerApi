using AutoMapper;
using CommonModels.Enum;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;

namespace CustomerCustomerApi.Services
{
    public class RbacActionSvc : IRbacActionSvc
    {
        private readonly IMapper _mapper;
        private readonly IRepository<RbacActionCosmosDb> _actionCosmosRepository;

        public RbacActionSvc(IMapper mapper,
                             IRepository<RbacActionCosmosDb> actionCosmosRepository)
        {
            _mapper = mapper;
            _actionCosmosRepository = actionCosmosRepository;
        }

        public async Task<RbacActionResponse> CreateActionAsync(RbacActionModel action, CancellationToken cancellationToken)
        {
            try
            {
                if (await _actionCosmosRepository.ExistsAsync(a => a.Category == action.Category.ToString()
                                                              && a.Action == action.Action, cancellationToken))
                    throw new InvalidDataException("An RBAC action with that category and action already exists");

                var actionItem = _mapper.Map<RbacActionCosmosDb>(action);
                var result = await _actionCosmosRepository.CreateAsync(actionItem, cancellationToken);

                return _mapper.Map<RbacActionResponse>(result);
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
                throw new RBACException("Error during creating RBAC action, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException($"Could not creating RBAC action. Look at the inner exception.", ex);
            }
        }

        public async Task<List<RbacActionResponse>> GetActionsByCategoryAsync(ActionCategory? category, CancellationToken cancellationToken)
        {
            try
            {
                var actions = new List<RbacActionCosmosDb>();

                if (category is null)
                {
                    actions = (await _actionCosmosRepository.GetAsync(x => true, cancellationToken)).ToList();
                }
                else
                {
                    actions = (await _actionCosmosRepository.GetAsync(x => x.Category == category.Value.ToString(), cancellationToken)).ToList();
                }

                return _mapper.Map<List<RbacActionResponse>>(actions);
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
                throw new RBACException($"Could not getting RBAC action. Look at the inner exception.", ex);
            }
        }

        public async Task<RbacActionResponse> UpdateActionAsync(string actionId, RbacActionModel action, CancellationToken cancellationToken)
        {
            try
            {
                var actionItems = await _actionCosmosRepository.GetAsync(x => x.Id == actionId, cancellationToken);
                if (!actionItems.Any())
                    throw new NotFoundExeption($"Could not find action. Look at the inner exception. Action id: {actionId}");

                var actionItem = actionItems.First();

                if (await _actionCosmosRepository.ExistsAsync(a => a.Category == action.Category.ToString()
                                              && a.Action == action.Action && a.Id != actionId, cancellationToken))
                    throw new InvalidDataException("An RBAC action with that category and action already exists");

                actionItem.Update(action.Description, action.Action);

                await _actionCosmosRepository.UpdateAsync(actionItem, cancellationToken: cancellationToken);

                return _mapper.Map<RbacActionResponse>(actionItem);
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
                throw new RBACException("Error during updating RBAC action, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException($"Could not updating RBAC action. Look at the inner exception.", ex);
            }
        }

        public async Task RemoveActionAsync(string actionId, CancellationToken cancellationToken)
        {
            try
            {
                var actionItems = await _actionCosmosRepository.GetAsync(x => x.Id == actionId, cancellationToken);
                if (!actionItems.Any())
                    throw new NotFoundExeption($"Could not find action. Look at the inner exception. Action id: {actionId}");

                var actionItem = actionItems.First();
                actionItem.ToggleIsDeleted();
                await _actionCosmosRepository.UpdateAsync(actionItem, cancellationToken: cancellationToken);
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
                throw new RBACException("Error during deleting RBAC action, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException($"Could not deleting RBAC action. Look at the inner exception.", ex);
            }
        }
    }
}
