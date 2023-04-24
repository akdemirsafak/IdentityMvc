using Microsoft.AspNetCore.Identity;
using Identity.Core.Models;

namespace Identity.Repository.Models;

public class AppUser : IdentityUser
{
    //ekstra propertyler 
    public string? City { get; set; }
    public string? Picture { get; set; }
    public DateTime? BirthDate { get; set; }
    public Gender? Gender { get; set; }

}