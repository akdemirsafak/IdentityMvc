using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IdentityMvc.Requirements;

public class ViolenceRequirement : IAuthorizationRequirement
{
    public int ThresholdAge { get; set; }
}

public class ViolenceRequirementHandler : AuthorizationHandler<ViolenceRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViolenceRequirement requirement)
    {
        var hasExchangeExpireClaim = context.User.HasClaim(x => x.Type == "BirthDate");
        if (!hasExchangeExpireClaim)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        Claim birthDateClaim = context.User.FindFirst("BirthDate");

        var today = DateTime.Now;
        DateTime birthDate = Convert.ToDateTime(birthDateClaim.Value);
        var age = today.Year - birthDate.Year;

        //Her 4 senede 1 şubat 29 olduğu için Artık yıl hesabı yapıyoruz.
        if (birthDate > today.AddYears(-age)) age--; //ARTIK YIL HESAPLAMASIDIR

        if (requirement.ThresholdAge > age) //Kullanıcının yaşı belirlediğimiz yaştan küçükse
        {
            context.Fail();
            return Task.CompletedTask;
        }
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}