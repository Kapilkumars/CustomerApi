using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.Module;
using CustomerCustomerApi.ResponseHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.ComponentModel.DataAnnotations;
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
    [ApiExplorerSettings(GroupName = "Modules")]
    [Route("api/v1.0/modules")]
    [ApiVersion("1.0")]
    public class ModulesController : ApiControllerBase
    {
        private readonly IModuleSvc _moduleSvc;
        public ModulesController(IModuleSvc moduleSvc,
                                ILogger<ModulesController> logger,
                                ICorrelationIdGenerator correlationIdGenerator)
                                : base(logger, correlationIdGenerator)
        {
            _moduleSvc = moduleSvc;
        }

        /// <summary>
        /// Obtain module by id and partition key
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{moduleId}")]
        [ProducesResponseType(typeof(ModuleResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetModuleAsync([Required][FromRoute] string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                var module = await _moduleSvc.GetModuleAsync(moduleId, cancellationToken);
                return Ok(module);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while obtain module", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtain module", ex.Message, "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Obtain all modules
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(List<ModuleResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllModulesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var modules = await _moduleSvc.GetAllModulesAsync(cancellationToken);
                if (modules is null || !modules.Any())
                    return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure modules list is refreshed!", HttpContext));
                else
                    return Ok(modules);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while obtaining modules", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining modules", apiResponseMessage: ex.Message, "Problem otaining modules, contact support.");
            }
        }

        /// <summary>
        /// Create module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(typeof(ModuleResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateModulesAsync([FromBody] ModuleModel module, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _moduleSvc.CreateModuleAsync(module, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while creating the module", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (InvalidDataException ex)
            {
                return ErrorProblem(ex, apiResponseMessage: ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while creating the module", apiResponseMessage: ex.Message, "Problem creating the module, contact support.");
            }
        }

        /// <summary>
        /// Update module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="moduleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{moduleId}")]
        [ProducesResponseType(typeof(ModuleResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateModuleAsync([Required][FromRoute] string moduleId, [FromBody] ModuleModel module, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _moduleSvc.UpdateModuleAsync(moduleId, module, cancellationToken);
                return Ok(result);
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while updating the module", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the module", apiResponseMessage: ex.Message, "Problem updating the module, contact support.");
            }
        }

        /// <summary>
        /// Delete module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{moduleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteActionAsync([Required][FromRoute] string moduleId, CancellationToken cancellationToken)
        {
            try
            {
                await _moduleSvc.RemoveModuleAsync(moduleId, cancellationToken);
                return NoContent();
            }
            catch (CosmosException ex)
            {
                return ErrorProblem(ex, ex.StatusCode, "Unexpected Exception while removing the module", apiResponseMessage: ex.Message, "Unexpected errors occurred.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while removing the module", ex.Message, "Unexpected errors occurred.");
            }
        }

    }
}