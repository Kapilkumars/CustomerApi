//namespace UserManagement;

//using CustomerCustomerApi.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.Azure.Cosmos;
//using Newtonsoft.Json.Linq;


//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
//public class AuthorizeAttribute : Attribute, IAuthorizationFilter
//{
//    public void OnAuthorization(AuthorizationFilterContext context)
//    {
//        // skip authorization if action is decorated with [AllowAnonymous] attribute
//        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
//        if (allowAnonymous)
//            return;

//        // authorization
//        var util = new JwtUtils();
//        //var token = HttpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
//        var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last(); ;
//        var userId = util.ValidateToken(token);
//        if (userId == null)
//            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
//    }
//}