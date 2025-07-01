using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CustomerCustomerApi.ResponseHelpers
{
    public class ProblemDetails409Conflict : ProblemDetails
    {
        public ProblemDetails409Conflict(string title, string detail, HttpContext httpContext)
        {
            Status = StatusCodes.Status404NotFound;
            Type = "https://www.rfc-editor.org/rfc/rfc2616#section-10.4.10";
            Title = title;
            Detail = detail;
            Instance = httpContext.Request.Path;
            Extensions.Add("traceId", httpContext.TraceIdentifier);
            Code = "NotFound";
        }

        [JsonProperty("code")] 
        public string Code { get; set; }
    }
}
