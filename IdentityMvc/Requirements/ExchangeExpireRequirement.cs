using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IdentityMvc.Requirements
{
    public class ExchangeExpireRequirement : IAuthorizationRequirement
    {
        
    }

    public class ExchangeExpireRequirementHandler : AuthorizationHandler<ExchangeExpireRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExchangeExpireRequirement requirement)
        {
            var hasExchangeExpireClaim = context.User.HasClaim(x => x.Type == "ExchangeExpireData");
            if (!hasExchangeExpireClaim)
            {
                context.Fail();
                return Task.CompletedTask;
            }

                Claim exchangeExpireDate = context.User.FindFirst("ExchangeExpireData")!;
            if (DateTime.Now>Convert.ToDateTime(exchangeExpireDate.Value))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}
