using Microsoft.AspNetCore.Identity;

namespace IdentityMvc.Localizations;

public class LocalizationIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
            { Code = "DuplicateUserName", Description = $"'{userName}' kullanıcı adı zaten kullanılıyor." };
        //return base.DuplicateUserName(userName);
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError { Code = "DuplicateEmail", Description = $"'{email}' adresine kayıtlı hesap var." };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError { Code = "PasswordTooShort", Description = "Parola en az 6 karakter olmalıdır." };
    }
}