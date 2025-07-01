using Microsoft.Identity.Web;
using System.Security.Claims;

namespace CustomerCustomerApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetIdentityId(this ClaimsPrincipal user)
        {
            if (!user.HasClaim(c => c.Type == ClaimConstants.ObjectId))
                throw new InvalidDataException("No graph user id in access token.");

            return user.Claims.First(c => c.Type == ClaimConstants.ObjectId).Value;
        }
    }
}
