using Identity.Core.Models;

namespace Identity.Core.ViewModels;

public class UserViewModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PictureUrl { get; set; }
    public string? City { get; set; }
    public Gender? Gender { get; set; }
}