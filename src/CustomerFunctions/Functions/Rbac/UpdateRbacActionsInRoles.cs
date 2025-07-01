using CommonModels.Enum;
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
    public class UpdateRbacActionsInRoles
    {
        private readonly IRepository<RoleCosmosDb> _roleRepository;
        private readonly IRepository<RbacActionCosmosDb> _actionRepository;

        public UpdateRbacActionsInRoles(IRepository<RoleCosmosDb> roleRepository,
                                        IRepository<RbacActionCosmosDb> actionRepository)
        {
            _roleRepository = roleRepository;
            _actionRepository = actionRepository;
        }
        [FunctionName("UpdateRbacActionsInRoles")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "metis-customers",
            collectionName: "rbac-actions",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            ILogger log)
        {
            try
            {
                if (input != null && input.Count > 0)
                {
                    log.LogInformation("UpdateRbacActionsInRoles function started!");
                    foreach (var inputItem in input)
                    {
                        var actionItem = JsonConvert.DeserializeObject<RbacActionCosmosDb>(inputItem.ToString());

                        if (actionItem.IsDeleted)
                        {
                            log.LogInformation("Start removing action from roles");
                            await RemoveActionFromRolesAsync(actionItem, log);
                        }
                        else
                        {
                            log.LogInformation("Start updating actions in roles");
                            await UpdateActionInRolesAsync(actionItem, log);
                        }
                    }
                    log.LogInformation("Documents modified " + input.Count);
                }
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
            }
        }

        public async Task RemoveActionFromRolesAsync(RbacActionCosmosDb actionItem, ILogger logger)
        {
            await _actionRepository.DeleteAsync(actionItem);
            logger.LogInformation("Action with Id " + actionItem.Id + " successfully deleted");

            var roles = await _roleRepository.GetAsync(x => x.Permissions.UiActions.Any(u => u.Actions.Any(a => a.Id == actionItem.Id))
                            || x.Permissions.DataActions.Any(u => u.Actions.Any(a => a.Id == actionItem.Id)));

            foreach (var roleItem in roles)
            {
                if (actionItem.Category == ActionCategory.uiAction.ToString())
                {
                    foreach (var ui in roleItem.Permissions.UiActions)
                    {
                        for (int i = ui.Actions.Count - 1; i >= 0; i--)
                        {
                            var action = ui.Actions[i];
                            if (action.Id == actionItem.Id)
                            {
                                ui.Actions.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var data in roleItem.Permissions.DataActions)
                    {
                        for (int i = data.Actions.Count - 1; i >= 0; i--)
                        {
                            var action = data.Actions[i];
                            if (action.Id == actionItem.Id)
                            {
                                data.Actions.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            
            await _roleRepository.UpdateAsync(roles);
            logger.LogInformation("Modified " + roles.Count() + " roles items");
        }

        public async Task UpdateActionInRolesAsync(RbacActionCosmosDb actionItem, ILogger logger)
        {
            var roles = await _roleRepository.GetAsync(x => x.Permissions.UiActions.Any(u => u.Actions.Any(a => a.Id == actionItem.Id))
                            || x.Permissions.DataActions.Any(u => u.Actions.Any(a => a.Id == actionItem.Id)));

            foreach (var roleItem in roles)
            {
                if (actionItem.Category == ActionCategory.uiAction.ToString())
                {
                    foreach (var ui in roleItem.Permissions.UiActions)
                    {
                        foreach (var action in ui.Actions)
                        {
                            if (action.Id == actionItem.Id)
                                action.Update(actionItem.Description, actionItem.Action);
                        }
                    }
                }
                else
                {
                    foreach (var data in roleItem.Permissions.DataActions)
                    {
                        foreach (var action in data.Actions)
                        {
                            if (action.Id == actionItem.Id)
                                action.Update(actionItem.Description, actionItem.Action);
                        }
                    }
                }
            }
            await _roleRepository.UpdateAsync(roles);
            logger.LogInformation("Modified " + roles.Count() + " roles items");
        }
    }
}
