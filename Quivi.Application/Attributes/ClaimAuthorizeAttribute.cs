using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Quivi.Application.Attributes
{
    public class ClaimAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string claimName;

        public ClaimAuthorizeAttribute(string claimName)
        {
            this.claimName = claimName;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if(context.HttpContext.User.HasClaim(c => c.Type == claimName) == false)
                context.Result = new ForbidResult();
            return Task.CompletedTask;
        }
    }
}
