using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models.Customer;
using CustomerCustomerApi.Models.User;
using CustomerCustomerApi.ResponseHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Threading;

namespace CustomerCustomerApi.Controllers;

/// <summary>
/// Manage users in Customer Systems, including Metis Control Users, Cloud Portal Users and Graph users
/// </summary>
[Authorize(AuthenticationSchemes = "MetisB2C,CloudPortalB2C,ApiKey")]
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails404NotFound), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
[ApiExplorerSettings(GroupName = "Users")]
[Route("api/v1.0")]
[ApiVersion("1.0")]
public class UsersController : ApiControllerBase
{
    private readonly IUserSvc _userSvc;
    private readonly ICustomerSvc _customerSvc;

    public UsersController(ILogger<UsersController> logger,
                          ICorrelationIdGenerator correlationIdGenerator,
                          IUserSvc userSvc,
                          ICustomerSvc customerSvc)
                          : base(logger, correlationIdGenerator)
    {
        _userSvc = userSvc;
        _customerSvc = customerSvc;
    }

    /// <summary>
    /// Create and user in Customer and AAD for desired B2C tenant
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = "MetisB2C")]
    [HttpPost("users")]
    [ProducesResponseType(typeof(MetisUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult> CreateUserAsync(MetisUser user, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _userSvc.CreateUserAsync(user, cancellationToken);
            return Ok(created);
        }
        catch (UserSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Problem creating user in the UserSvc", "Problem creating the user, contact support.", ex.Message);
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected Exception while creating the user", "Problem creating the user, contact support.", "Unexpected errors occurred.");
        }
    }

    /// <summary>
    /// Obtain Metis user that also provides Azure B2C tenant Graph related information
    /// </summary>
    /// <returns></returns>
    [HttpGet("user")]
    [ProducesResponseType(typeof(MetisUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUserProfileAsync(CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userSvc.GetUserAsync(cancellationToken);
            if (user == null)
                return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure user list is refreshed!", HttpContext));
            else
                return Ok(user);
        }
        catch (UserSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Could not obtain user in the UserSvc", "Problem with getting the user, contact support.", "Make sure that a user with this Id exists in the system.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected Exception while obtaining the user", "Problem with getting the user, contact support.", "Unexpected errors occurred.");
        }
    }

    /// <summary>
    /// Obtain Metis user that also provides Azure B2C tenant Graph related information
    /// </summary>
    /// <returns></returns>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(MetisUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetUserAsync([FromRoute] string userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userSvc.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
                return NotFound(new ProblemDetails404NotFound("Not Found in DB", $"Ensure user list is refreshed!", HttpContext));
            else
                return Ok(user);
        }
        catch (UserSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Could not obtain user in the UserSvc", "Problem with getting the user, contact support.", "Make sure that a user with this Id exists in the system.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected Exception while obtaining the user", "Problem with getting the user, contact support.", "Unexpected errors occurred.");
        }
    }
    /// <summary>
    /// Get all Metis Users by customer Id or by the Graph User Id including their Azure B2C tenant Graph information
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="graphUserId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<MetisUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllUsersAsync(CancellationToken cancellationToken, [FromQuery] string? customerId, [FromQuery] string? graphUserId)
    {
        List<MetisUserResponse> users = new List<MetisUserResponse>();

        try
        {
            if (!string.IsNullOrEmpty(graphUserId))
            {
                users = await _userSvc.GetUserByGraphUserIdAsync(graphUserId);

                if (users.Any())
                    return Ok(users);
                else
                    return NotFound(new ProblemDetails404NotFound("Not Found", $"Ensure user list is refreshed!", HttpContext));
            }
            else if (!string.IsNullOrEmpty(customerId))
            {
                users = await _userSvc.GetUsersByCustomerIdAsync(customerId, cancellationToken);

                if (users.Any())
                    return Ok(users);
                else
                    return NotFound(new ProblemDetails404NotFound("Not Found", $"Ensure user list is refreshed!", HttpContext));
            }
            else
            {
                users = await _userSvc.GetAllUsersAsync(cancellationToken);

                if (users.Any())
                    return Ok(users);
                else
                    return NotFound(new ProblemDetails404NotFound("Not Found", $"Ensure user list is refreshed!", HttpContext));
            }
        }
        catch (UserSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, loggingDetail: $"Could not obtain users in the UserSvc with the graphUserId: {graphUserId} or customerId: {customerId}", "Problem with getting users, contact support.", "Make sure that users exist in the system.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, loggingDetail: $"Could not obtain users in the UserSvc with the graphUserId: {graphUserId} or customerId: {customerId}", "Problem with getting users, contact support.", "Unexpected errors occurred.");
        }
    }

    /// <summary>
    /// Delete user in Customer and AAD Graph for the B2C tenant
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DeleteUserAsync([FromRoute] string userId, CancellationToken cancellationToken)
    {
        try
        {
            await _userSvc.RemoveUserAsync(userId, cancellationToken);
            return Accepted();
        }
        catch (UserSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Could not delete the user in the UserSvc", "Problem with deleting users, contact support.", "Make sure that users with this Id exist in the system.");
        }
        catch (Exception ex)
        {
            return ErrorProblem(ex, "Unexpected Exception while deleting the user", "Problem with deleting the user, contact support.", "Unexpected errors occurred.");
        }
    }

    /// <summary>
    /// Update user in Customer 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("users/{userId}")]
    [ProducesResponseType(typeof(MetisUserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateAsync([FromRoute] string userId, [FromBody] MetisUser userModel, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userSvc.UpdateAsync(userModel, userId, userModel.Admin, cancellationToken);
            return Ok(user);
        }
        catch (UserSvcException ex)
        {
            return ErrorProblem(ex, ex.HttpStatusCode, "Problem updating user in the UserSvc", "Problem updating the user, contact support.", "Ensure that customer with this Id exist in the system.");
        }
        catch (Exception e)
        {
            return ErrorProblem(e, "Unexpected Exception while updating the user", "Problem updating the user, contact support.", "Unexpected errors occurred.");
        }
    }
}