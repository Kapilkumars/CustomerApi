using CustomerCustomerApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerCustomerFunctions.Functions.Rbac
{
    public class UpdateRbacResourcesInRoles
    {
        private readonly IRepository<RoleCosmosDb> _roleRepository;
        private readonly IRepository<RbacResourceCosmosDb> _resourceRepository;
        public UpdateRbacResourcesInRoles(IRepository<RoleCosmosDb> roleRepository,
            IRepository<RbacResourceCosmosDb> resourceRepository)
        {
            _roleRepository = roleRepository;
            _resourceRepository = resourceRepository;
        }
        [FunctionName("UpdateRbacResourcesInRoles")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "metis-customers",
            collectionName: "rbac-resources",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            ILogger log)
        {
            try
            {
                if (input != null && input.Count > 0)
                {
                    log.LogInformation("UpdateRbacResourcesInRoles function started!");
                    foreach (var inputItem in input) 
                    {
                        var resourceItem = JsonConvert.DeserializeObject<RbacResourceCosmosDb>(inputItem.ToString());
                        if (resourceItem.IsDeleted)
                        {
                            log.LogInformation("Start removing resource from roles");
                            await RemoveResourcesFromRolesAsync(resourceItem, log);
                        }
                        else
                        {
                            log.LogInformation("Start updating resouces in roles");
                            await UpdateResourcesInRolesAsync(resourceItem, log);
                        }
                    }
                    log.LogInformation("Documents modified " + input.Count);
                }
            }
            catch (CosmosException ex)
            {
                log.LogInformation(ex.Message);
            }
            catch (System.Exception ex)
            {
                log.LogInformation(ex.Message);
            }
        }

        public async Task UpdateResourcesInRolesAsync(RbacResourceCosmosDb resourceItem, ILogger logger)
        {
            var roles = await _roleRepository.GetAsync(x => x.Permissions.UiActions.Any(u => u.Resource.Id == resourceItem.Id)
                           || x.Permissions.DataActions.Any(d => d.Resource.Id == resourceItem.Id));

            if (roles.Count() > 0)
            {
                foreach (var roleItem in roles)
                {
                    foreach (var ui in roleItem.Permissions.UiActions)
                    {
                        if (ui.Resource.Id == resourceItem.Id)
                        {
                            ui.Resource.Update(resourceItem.Description);
                        }
                    }
                    foreach (var data in roleItem.Permissions.DataActions)
                    {
                        if (data.Resource.Id == resourceItem.Id)
                        {
                            data.Resource.Update(resourceItem.Description);
                        }
                    }
                }
                await _roleRepository.UpdateAsync(roles);
            }
            logger.LogInformation("Modified " + roles.Count() + " roles items");
        }

        public async Task RemoveResourcesFromRolesAsync(RbacResourceCosmosDb resourceItem, ILogger logger)
        {
            await _resourceRepository.DeleteAsync(resourceItem);
            logger.LogInformation("Resource with Id " + resourceItem.Id + " successfully deleted");

            var roles = await _roleRepository.GetAsync(x => x.Permissions.UiActions.Any(u => u.Resource.Id == resourceItem.Id)
            || x.Permissions.DataActions.Any(d => d.Resource.Id == resourceItem.Id));

            if (roles.Count() > 0)
            {
                foreach (var roleItem in roles)
                {
                    for (int i = roleItem.Permissions.UiActions.Count - 1; i >= 0; i--)
                    {
                        var ui = roleItem.Permissions.UiActions[i];
                        if (ui.Resource.Id == resourceItem.Id)
                        {
                            roleItem.Permissions.UiActions.RemoveAt(i);
                        }
                    }

                    for (int i = roleItem.Permissions.DataActions.Count - 1; i >= 0; i--)
                    {
                        var data = roleItem.Permissions.DataActions[i];
                        if (data.Resource.Id == resourceItem.Id)
                        {
                            roleItem.Permissions.DataActions.RemoveAt(i);
                        }
                    }
                }
                await _roleRepository.UpdateAsync(roles);
            }
            logger.LogInformation("Modified " + roles.Count() + " roles items");
        }
    }
}
