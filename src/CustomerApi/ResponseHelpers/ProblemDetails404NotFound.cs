using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CustomerCustomerApi.ResponseHelpers
{
    public class ProblemDetails404NotFound : ProblemDetails
    {
        public ProblemDetails404NotFound(string title, string detail, HttpContext httpContext)
        {
            Status = StatusCodes.Status404NotFound;
            Type = "https://tools.ietf.org/html/rfc2616#section-10.4.5";
            Title = title;
            Detail = detail;
            Instance = httpContext.Request.Path;
            Extensions.Add("traceId", httpContext.TraceIdentifier);
            Code = "NotFound";
        }

        [JsonProperty("code")] public string Code { get; set; }
    }
}
