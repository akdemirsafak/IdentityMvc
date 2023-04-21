using IdentityMvc.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityMvc.CustomValidations;

public class PasswordValidator : IPasswordValidator<AppUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    {
        var errors = new List<IdentityError>();
        if (password!.ToLower().Contains(user.UserName.ToLower()))
            errors.Add(new IdentityError
                { Code = "PasswordContainUserName", Description = "Şifre kullanıcı adı içeremez." });

        if (password!.StartsWith("1234"))
            errors.Add(new IdentityError
                { Code = "PasswordContainStart1234", Description = "Şifre 1234 ile başlayamaz." });

        if (errors.Any()) return Task.FromResult(IdentityResult.Failed());
        return Task.FromResult(IdentityResult.Success);
    }
}