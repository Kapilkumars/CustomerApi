using AutoMapper;
using CommonModels.Enum;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using System.Net;
using ActionInfo = CustomerCustomerApi.Models.ActionInfo;

namespace CustomerCustomerApi.Services
{
    public class RoleSvc : IRoleSvc
    {
        private readonly IMapper _mapper;
        private readonly IRepository<RoleCosmosDb> _roleCosmosRepository;
        private readonly IRepository<RbacActionCosmosDb> _actionCosmosRepository;
        private readonly IRepository<RbacResourceCosmosDb> _resourceCosmosRepository;
        public RoleSvc(IMapper mapper,
                       IRepository<RoleCosmosDb> roleCosmosRepository,
                       IRepository<RbacActionCosmosDb> actionCosmosRepository,
                       IRepository<RbacResourceCosmosDb> resourceCosmosRepository)
        {
            _mapper = mapper;
            _roleCosmosRepository = roleCosmosRepository;
            _actionCosmosRepository = actionCosmosRepository;
            _resourceCosmosRepository = resourceCosmosRepository;
        }

        public async Task<RoleResponse> CreateRoleAsync(RoleModel role, CancellationToken cancellationToken)
        {
            try
            {
                var roleItem = _mapper.Map<RoleCosmosDb>(role);
                roleItem.Permissions = new RolePermision 
                {
                    DataActions = new List<ActionInfo>(),
                    UiActions = new List<ActionInfo>()
                };

                foreach (var item in role.Permission.UiActions)
                {
                    var resourceItem = await _resourceCosmosRepository.GetAsync(x => x.Id == item.ResourceId, cancellationToken: cancellationToken);
                    if (!resourceItem.Any())
                        throw new RBACException($"Resource with id {item.ResourceId} for UIAction is not exists", HttpStatusCode.NotFound);

                    var actionItems = new List<RbacActionCosmosDb>();
                    foreach (var actionId in item.ActionIds)
                    {
                        var items = await _actionCosmosRepository.GetAsync(a => a.Id == actionId && 
                                                    a.Category == nameof(ActionCategory.uiAction), cancellationToken);
                        if (!items.Any())
                            throw new RBACException($"Action with id {actionId} for UIAction is not exist", HttpStatusCode.NotFound);
                        else
                            actionItems.Add(items.First());
                    }

                    var permeation = new ActionInfo
                    {
                        Resource = resourceItem.First(),
                        Actions = actionItems.ToList(),

                    };

                    roleItem.Permissions.UiActions.Add(permeation);
                }

                foreach (var item in role.Permission.DataActions)
                {
                    var resourceItem = await _resourceCosmosRepository.GetAsync(x => x.Id == item.ResourceId, cancellationToken: cancellationToken);
                    if (!resourceItem.Any())
                        throw new RBACException($"Resource with id {item.ResourceId} for DataAction is not exists", HttpStatusCode.NotFound);

                    var actionItems = new List<RbacActionCosmosDb>();
                    foreach (var actionId in item.ActionIds)
                    {
                        var items = await _actionCosmosRepository.GetAsync(a => a.Id == actionId && a.Category == nameof(ActionCategory.dataAction), cancellationToken);
                        if (!items.Any())
                            throw new RBACException($"Action with id {actionId} for DataAction is not exist", HttpStatusCode.NotFound);
                        else
                            actionItems.Add(items.First());
                    }

                    var permeation = new ActionInfo
                    {
                        Resource = resourceItem.First(),
                        Actions = actionItems.ToList(),

                    };
                    roleItem.Permissions.DataActions.Add(permeation);
                }

                var result = await _roleCosmosRepository.CreateAsync(roleItem, cancellationToken);
                return _mapper.Map<RoleResponse>(result);
            }
            catch(CosmosException ex)
            {
                throw new RBACException("Cosmos related exception, see inner exception!", ex)
                {
                    HttpStatusCode = ex.StatusCode
                };
            }
            catch (RBACException ex)
            {
                throw new RBACException("Error during creating role, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task<List<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var roleItems = await _roleCosmosRepository.GetAsync(x => true, cancellationToken);
                return _mapper.Map<List<RoleResponse>>(roleItems);
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
                throw new RBACException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task<RoleResponse> UpdateRoleAsync(string roleId, RoleModel role, CancellationToken cancellationToken)
        {
            try
            {
                var roleItems = await _roleCosmosRepository.GetAsync(x => x.Id == roleId, cancellationToken);
                if (!roleItems.Any())
                    throw new RBACException($"Could not find role. Look at the inner exception. Role id: {roleId}", HttpStatusCode.NotFound);

                var roleItem = roleItems.First();
                roleItem.Permissions.UiActions.Clear();
                roleItem.Permissions.DataActions.Clear();

                foreach (var item in role.Permission.UiActions)
                {
                    var resourceItem = await _resourceCosmosRepository.GetAsync(x => x.Id == item.ResourceId, cancellationToken: cancellationToken);
                    if (!resourceItem.Any())
                        throw new RBACException($"Resource with id {item.ResourceId} for UIAction is not exists", HttpStatusCode.NotFound);

                    var actionItems = new List<RbacActionCosmosDb>();
                    foreach (var actionId in item.ActionIds)
                    {
                        var items = await _actionCosmosRepository.GetAsync(a => a.Id == actionId &&
                                                    a.Category == nameof(ActionCategory.uiAction), cancellationToken);
                        if (!items.Any())
                            throw new RBACException($"Action with id {actionId} for UIAction is not exist", HttpStatusCode.NotFound);
                        else
                            actionItems.Add(items.First());
                    }

                    var permeation = new ActionInfo
                    {
                        Resource = resourceItem.First(),
                        Actions = actionItems.ToList(),

                    };

                    roleItem.Permissions.UiActions.Add(permeation);
                }

                foreach (var item in role.Permission.DataActions)
                {
                    var resourceItem = await _resourceCosmosRepository.GetAsync(x => x.Id == item.ResourceId, cancellationToken: cancellationToken);
                    if (!resourceItem.Any())
                        throw new RBACException($"Resource with id {item.ResourceId} for DataAction is not exists", HttpStatusCode.NotFound);

                    var actionItems = new List<RbacActionCosmosDb>();
                    foreach (var actionId in item.ActionIds)
                    {
                        var items = await _actionCosmosRepository.GetAsync(a => a.Id == actionId && a.Category == nameof(ActionCategory.dataAction), cancellationToken);
                        if (!items.Any())
                            throw new RBACException($"Action with id {actionId} for DataAction is not exist", HttpStatusCode.NotFound);
                        else
                            actionItems.Add(items.First());
                    }

                    var permeation = new ActionInfo
                    {
                        Resource = resourceItem.First(),
                        Actions = actionItems.ToList(),

                    };
                    roleItem.Permissions.DataActions.Add(permeation);
                }
                roleItem.Update(_mapper.Map<RoleProperties>(role.Properties));

                await _roleCosmosRepository.UpdateAsync(roleItem, cancellationToken: cancellationToken);

                return _mapper.Map<RoleResponse>(roleItem);
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
                throw new RBACException("Error during updating role, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException("Not cosmos related exception, see inner exception!", ex);
            }
        }

        public async Task RemoveRoleAsync(string roleId, CancellationToken cancellationToken)
        {
            try
            {
                await _roleCosmosRepository.DeleteAsync(roleId, cancellationToken: cancellationToken);
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
                throw new RBACException("Error during deleting role, see inner exception!", ex)
                {
                    HttpStatusCode = ex.HttpStatusCode
                };
            }
            catch (Exception ex)
            {
                throw new RBACException("Not cosmos related exception, see inner exception!", ex);
            }
        }
    }
}
