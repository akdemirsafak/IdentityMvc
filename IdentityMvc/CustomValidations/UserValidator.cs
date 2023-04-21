using IdentityMvc.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityMvc.CustomValidations;

public class UserValidator : IUserValidator<AppUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
    {
        //Identity default olarak o kullanıcı adı kullanılıyorsa error üretme özelliğine sahiptir.
        //ekstra bir business e gerek duyulmaz.
        var errors = new List<IdentityError>();
        var isDigit =
            int.TryParse(user.UserName[0]!.ToString(),
                out _); //değişken ismi değil de _ kullandığımızda memory'de herhangi bir değişken allocite edilmeyecek(yer ayrılmayacak)
        if (isDigit)
            errors.Add(new IdentityError
            {
                Code = "UserNameContainFirstLetterDigit", Description = "Kullanıcı adının ilk karakteri sayı olamaz."
            });

        if (errors.Any()) return Task.FromResult(IdentityResult.Failed());
        return Task.FromResult(IdentityResult.Success);
    }
}