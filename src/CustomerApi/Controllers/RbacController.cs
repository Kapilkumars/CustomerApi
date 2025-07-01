using CommonModels.Enum;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.Rbac;
using CustomerCustomerApi.ResponseHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Net.Mime;

namespace CustomerCustomerApi.Controllers
{
    [Authorize(AuthenticationSchemes = "MetisB2C,CloudPortalB2C")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails404NotFound), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ApiExplorerSettings(GroupName = "RBAC")]
    [Route("api/v1.0/rbac")]
    [ApiVersion("1.0")]
    public class RbacController : ApiControllerBase
    {
        private readonly IRoleSvc _roleSvs;
        private readonly IRbacActionSvc _rbacActionSvc;
        private readonly IRbacResourceSvc _rbacResourceSvc;
        public RbacController(IRoleSvc roleSvs,
                              IRbacResourceSvc rbacResourceSvc,
                              IRbacActionSvc rbacActionSvc,
                              ILogger<RbacController> logger,
                              ICorrelationIdGenerator correlationIdGenerator)
                              : base(logger, correlationIdGenerator)
        {
            _roleSvs = roleSvs;
            _rbacActionSvc = rbacActionSvc;
            _rbacResourceSvc = rbacResourceSvc;
        }

        #region Role

        /// <summary>
        /// Obtain all roles 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetRoleAsync(CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _roleSvs.GetAllRolesAsync(cancellationToken);
                if (roles is null || !roles.Any())
                    return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure roles list is refreshed!", HttpContext));
                else
                    return Ok(roles);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, $"Unexpected Exception while obtaining roles.", apiResponseMessage: ex.Message);
            }
            catch (RBACException ex)
            {
                return ErrorProblem(ex, loggingDetail: "Problem obtaining roles in the RoleSvs.", apiResponseMessage: ex.Message, statusCode: ex.HttpStatusCode);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining roles", "Problem obtaining roles, contact support.", "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Create role in Customer
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("roles")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateRoleAsync(RoleModel role, CancellationToken cancellationToken)
        {
            try
            {
                var created = await _roleSvs.CreateRoleAsync(role, cancellationToken);
                return Ok(created);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, $"Unexpected Exception while creating the role.", apiResponseMessage: ex.Message);
            }
            catch (RBACException ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, $"Unexpected Exception while creating the role.", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while creating the role.", ex.Message, "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Update role in Customer, name cannot be updated because it partitionKey
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("roles/{roleId}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateRoleAsync([FromRoute] string roleId, [FromBody] RoleModel role, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _roleSvs.UpdateRoleAsync(roleId, role, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while update the role", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (RBACException ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, "Problem update the role in the RoleSvs", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the role", apiResponseMessage: ex.Message, "Problem update the role, contact support.");
            }
        }

        /// <summary>
        /// Delete role in Customer
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("roles/{roleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteRoleAsync([FromRoute] string roleId, CancellationToken cancellationToken)
        {
            try
            {
                await _roleSvs.RemoveRoleAsync(roleId, cancellationToken);
                return NoContent();
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while removing the role", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (RBACException ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, "Problem update the role in the RoleSvs", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while removing the role", apiResponseMessage: ex.Message);
            }
        }

        #endregion

        #region Resource

        /// <summary>
        /// Obtain all rbac resources
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("resources")]
        [ProducesResponseType(typeof(List<RbacResourceResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetResourcesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var resources = await _rbacResourceSvc.GetAllResourcesAsync(cancellationToken);
                if (resources is null || !resources.Any())
                    return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure resources list is refreshed!", HttpContext));
                else
                    return Ok(resources);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while obtaining resources", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (RBACException ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, "Problem delete the role in the RoleSvs", apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining resources", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Create rbac resource in Customer
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("resources")]
        [ProducesResponseType(typeof(RbacResourceResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateResourceAsync(RbacResourceModel resource, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _rbacResourceSvc.CreateResourceAsync(resource, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while creating the resource", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (InvalidDataException ex)
            {
                return ErrorProblem(ex, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while creating the resource", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Update resources in Customer
        /// </summary>
        /// /// <param name="resourceId"></param>
        /// <param name="resource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("resources/{resourceId}")]
        [ProducesResponseType(typeof(RbacResourceResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateResourceAsync([FromRoute] string resourceId, [FromBody] RbacResourceModel resource, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _rbacResourceSvc.UpdateResourceAsync(resourceId, resource, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while updating the resource", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (NotFoundExeption ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the resource", apiResponseMessage: ex.Message, "Problem creating the resource, contact support.");
            }
        }

        /// <summary>
        /// Delete rbac resource in Customer
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("resources/{resourceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteResourceAsync([FromRoute]string resourceId, CancellationToken cancellationToken)
        {
            try
            {
                await _rbacResourceSvc.RemoveResourceAsync(resourceId, cancellationToken);
                return NoContent();
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while removing the resource", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while removing the resource", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
        }

        #endregion

        #region Action

        /// <summary>
        /// Obtain all rbac action
        /// </summary>
        /// <param name="category"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("actions")]
        [ProducesResponseType(typeof(List<RbacActionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetActionsAsync(ActionCategory? category, CancellationToken cancellationToken)
        {
            try
            {
                var action = await _rbacActionSvc.GetActionsByCategoryAsync(category, cancellationToken);
                if (action is null || !action.Any())
                    return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure actions list is refreshed!", HttpContext));
                else
                    return Ok(action);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while obtaining actions", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining actions", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Create RBAC action in Customer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("actions")]
        [ProducesResponseType(typeof(RbacActionResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateActionAsync([FromBody] RbacActionModel action, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _rbacActionSvc.CreateActionAsync(action, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while creating the action", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (NotFoundExeption ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while creating the action", apiResponseMessage: ex.Message, "Problem creating the action, contact support.");
            }
        }

        /// <summary>
        /// Update action in Customer
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("actions/{actionId}")]
        [ProducesResponseType(typeof(RbacActionResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateActionAsync([FromRoute] string actionId, [FromBody] RbacActionModel action, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _rbacActionSvc.UpdateActionAsync(actionId, action, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while updating the action", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (NotFoundExeption ex)
            {
                return ErrorProblem(ex, statusCode: ex.HttpStatusCode, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the action", apiResponseMessage: ex.Message, "Problem with updating the action, contact support.");
            }
        }

        /// <summary>
        /// Delete RBAC action in Customer
        /// </summary>
        /// <param name="actionId"></param> 
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("actions/{actionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteActionAsync(string actionId, CancellationToken cancellationToken)
        {
            try
            {
                await _rbacActionSvc.RemoveActionAsync(actionId, cancellationToken);
                return NoContent();
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while removing the action", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while removing the action", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
        }

        #endregion
    }
}