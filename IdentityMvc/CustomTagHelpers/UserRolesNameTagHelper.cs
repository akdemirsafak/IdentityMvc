using System.Text;
using Identity.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityMvc.CustomTagHelpers;

[HtmlTargetElement("td", Attributes = "user-roles")]
public class UserRolesNameTagHelper : TagHelper
{  
    public UserManager<AppUser> UserManager { get; set; }  
    [HtmlAttributeName("user-roles")] public string UserId { get; set; }
    public UserRolesNameTagHelper(UserManager<AppUser> userManager)
    {
        UserManager = userManager;
    }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        //Bu method string veya html ifadeyi yukarıda htmltargetelement de belirttiğimiz şartlara uyan(html sayfasında td taginde  user-roles="" olan kısıma) yazar.

        var user = await UserManager.FindByIdAsync(UserId);
        var userRoles = await UserManager.GetRolesAsync(user!);
        var stringBuilder = new StringBuilder();
        userRoles.ToList().ForEach(x =>
        {
            stringBuilder.Append($"<span class='badge bg-secondary mx-1'>{x.ToLower()} </span>");
        });
        output.Content.SetHtmlContent(stringBuilder.ToString());    
        //return base.ProcessAsync(context, output);
    }
}