using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityMvc.CustomTagHelpers;

public class UserPictureThumbnailTagHelper : TagHelper
{
    public string? PictureUrl { get; set; }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "img";
        if (string.IsNullOrEmpty(PictureUrl))
        {
            output.Attributes.SetAttribute("src", "/userPictures/default-user-profile-picture.png");
        }
        else
        {
            output.Attributes.SetAttribute("src", $"/userPictures/{PictureUrl}");
        }
    }
}