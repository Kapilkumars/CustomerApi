using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.Customer;
using CustomerCustomerApi.ResponseHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CustomerCustomerApi.Controllers
{
    /// <summary>
    /// Manage customers in Customer Systems
    /// </summary>
    [Authorize(AuthenticationSchemes = "MetisB2C,CloudPortalB2C")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails404NotFound), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ApiExplorerSettings(GroupName = "Customers")]
    [Route("api/v1.0")]
    [ApiVersion("1.0")]
    public class CustomersController : ApiControllerBase
    {
        private readonly ICustomerSvc _customerSvc;
        public CustomersController(ILogger<CustomersController> logger,
                                  ICustomerSvc customerSvc,
                                  ICorrelationIdGenerator correlationIdGenerator)
                                  : base(logger, correlationIdGenerator)
        {
            _customerSvc = customerSvc;

        }

        /// <summary>
        /// Obtaining customer by id in Customer 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("customers/{customerId}")]
        [ProducesResponseType(typeof(CustomerResponseModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetCustomerByIdAsync([FromRoute] string customerId, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _customerSvc.GetCustomerByIdAsync(customerId, cancellationToken);
                return Ok(customer);
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem obtaining customers in the CustomerSvc", "Problem obtaining customers, contact support.", "Ensure that customers with that number exist in the system");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining customers", "Problem obtaining the customers, contact support.", "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Obtaining all customers in Customer 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("customers")]
        [ProducesResponseType(typeof(List<CustomerResponseModel>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllCustomersAsync(CancellationToken cancellationToken, [FromQuery] string? userId)
        {
            try
            {
                List<CustomerResponseModel> customers;
                if (!string.IsNullOrEmpty(userId))
                {
                    //filter customers by userId
                    customers = await _customerSvc.GetCustomersByUserIdAsync(userId, cancellationToken);
                } 
                else
                {
                    //fetch all customers
                    customers = await _customerSvc.GetAllCustomersAsync(cancellationToken);
                }
                return Ok(customers);
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem obtaining customers in the CustomerSvc", "Problem obtaining customers, contact support.", "Ensure that customers with that number exist in the system");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while obtaining customers", "Problem obtaining the customers, contact support.", "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Create and Customer in Customer 
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("customers")]
        [ProducesResponseType(typeof(CustomerResponseModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreateCustomerAsync(CreateCustomerModel customer, CancellationToken cancellationToken)
        {
            try
            {
                var created = await _customerSvc.CreateCustomerAsync(customer, cancellationToken);
                return Ok(created);
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem creating customer in the CustomerSvc", "Problem creating the customer, contact support.", ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while creating the customer", "Problem creating the customer, contact support.", "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Update customer in Customer 
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("customers/{customerId}")]
        [ProducesResponseType(typeof(CustomerResponseModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateCustomerAsync([FromRoute] string customerId, [FromBody] CustomerModel customer, CancellationToken cancellationToken)
        {
            try
            {
                var created = await _customerSvc.UpdateCustomerAsync(customerId, customer, cancellationToken);
                return Ok(created);
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem updating customer in the CustomerSvc", "Problem updating the customer, contact support.", ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the customer", "Problem updating the customer, contact support.", "Unexpected errors occurred.");
            }
        }


        /// <summary>
        /// Add tenant to customer in Customer 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("customers/{customerId}/tenant")]
        [ProducesResponseType(typeof(CustomerResponseModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> AddTenantAsync([FromRoute] string customerId, [FromBody] TenantModel tenant, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _customerSvc.AddTenantAsync(customerId, tenant, cancellationToken);
                return Ok(customer);
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem updating customer in the CustomerSvc", "Problem updating the customer, contact support.", ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the customer", "Problem updating the customer, contact support.", "Unexpected errors occurred.");
            }
        }


        /// <summary>
        /// Update tenant in customer in Customer 
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="customerId"></param>
        /// <param name="tenantId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("customers/{customerId}/tenant/{tenantId}")]
        [ProducesResponseType(typeof(CustomerResponseModel), StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateTenantAsync([FromRoute] string customerId, [FromRoute] string tenantId, [FromBody] TenantModel tenant, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _customerSvc.UpdateTenantAsync(customerId, tenantId, tenant, cancellationToken);
                return Ok(customer);
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem updating customer in the CustomerSvc", "Problem updating the customer, contact support.", ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the customer", "Problem updating the customer, contact support.", "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Delete tenant from customer in Customer 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("customers/{customerId}/tenant/{tenantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteTenantAsync([FromRoute] string customerId, [FromRoute] string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                await _customerSvc.DeleteTenantAsync(customerId, tenantId, cancellationToken);
                return NoContent();
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Problem updating customer in the CustomerSvc", "Problem updating the customer, contact support.", ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while updating the customer", "Problem updating the customer, contact support.", "Unexpected errors occurred.");
            }
        }

        /// <summary>
        /// Delete customer in Customer 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("customers/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteCustomerAsync([FromRoute] string customerId, CancellationToken cancellationToken)
        {
            try
            {
                await _customerSvc.RemoveCustomerAsync(customerId, cancellationToken);
                return Accepted();
            }
            catch (CustomerSvcException ex)
            {
                return ErrorProblem(ex, ex.HttpStatusCode, "Could not delete the customer in the CustomerSvc", "Problem with deleting customer, contact support.", "Make sure that customer with this Id exist in the system.");
            }
            catch (Exception ex)
            {
                return ErrorProblem(ex, "Unexpected Exception while deleting the customer", "Problem with deleting the customer, contact support.", "Unexpected errors occurred.");
            }
        }
    }
}