using Customer.Metis.Logging.Correlation;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CustomerCustomerApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    private readonly ILogger _logger;
    private readonly ICorrelationIdGenerator _correlationIdGenerator;
    public ApiControllerBase(ILogger logger, ICorrelationIdGenerator correlationIdGenerator)
    {
        _logger = logger;
        _correlationIdGenerator = correlationIdGenerator;
    }
    /// <summary>
    /// Return well descriptive error to the UI while not exposing the internals of the exceptions. Then log the details.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="loggingDetail"></param>
    /// <param name="apiResponseMessage"></param>
    /// <param name="apiResponseMessageTitle"></param>
    /// <returns></returns>
    internal ObjectResult ErrorProblem(Exception e, string? loggingDetail = null, string? apiResponseMessage = null, string? apiResponseMessageTitle = null)
    {
        //Log extra information
        _logger.LogError(e, $"{loggingDetail}., CorrelationId: {_correlationIdGenerator.Get()}");

        //Keep the response simple and do not expose inner workings
        return ErrorProblemResult(HttpStatusCode.InternalServerError, $"{apiResponseMessage}", apiResponseMessageTitle ?? "Generic Exception");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="loggingDetail"></param>
    /// <param name="apiResponseMessage"></param>
    /// <param name="apiResponseMessageTitle"></param>
    /// <returns></returns>
    internal ObjectResult ErrorProblem(string? loggingDetail = null, string? apiResponseMessage = null, string? apiResponseMessageTitle = null)
    {
        //Log extra information
        _logger.LogError($"{loggingDetail}., CorrelationId: {_correlationIdGenerator.Get()}");

        //Keep the response simple and do not expose inner workings
        return ErrorProblemResult(HttpStatusCode.InternalServerError, $"{apiResponseMessage}", apiResponseMessageTitle ?? "Generic Exception");
    }

    /// <summary>
    /// Return well descriptive error to the UI with status code while not exposing the internals of the exceptions. Then log the details.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="loggingDetail"></param>
    /// <param name="apiResponseMessage"></param>
    /// <param name="apiResponseMessageTitle"></param>
    /// <returns></returns>
    internal ObjectResult ErrorProblem(Exception e, HttpStatusCode? statusCode, string? loggingDetail = null, string? apiResponseMessage = null, string? apiResponseMessageTitle = null)
    {
        //Log extra information
        _logger.LogError(e, $"{loggingDetail}., CorrelationId: {_correlationIdGenerator.Get()}");

        //Keep the response simple and do not expose inner workings
        return ErrorProblemResult(statusCode, $"{apiResponseMessage}", apiResponseMessageTitle ?? "Generic Exception");
    }

    private ObjectResult ErrorProblemResult(HttpStatusCode? statusCode, string? apiResponseMessage = null, string? apiResponseMessageTitle = null)
    {
        var logReminder = "Look at the logs for more information about the exception.";
        //Keep the response simple and do not expose inner workings
        return Problem($"{apiResponseMessage}. {logReminder}" ?? $"We don't know the reason for this Exception in the API. {logReminder}", HttpContext.Request.Path, (int)statusCode, apiResponseMessageTitle ?? "Generic Exception", "https://tools.ietf.org/html/rfc2616#section-10.5.1");
    }
}
