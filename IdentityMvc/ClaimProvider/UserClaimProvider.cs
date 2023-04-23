using System.Security.Claims;
using IdentityMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityMvc.ClaimProvider;

public class UserClaimProvider : IClaimsTransformation //DIContainer'e geçtik.
{
    //Burada framework'ün davranışına müdahale ederek kendi istediğimiz dataları ekleriz. AUTHORIZE ÇALIŞMADAN ÖNCE BURASI ÇALIŞIR.
    //Dikkat edilmesi gereken kritik dataları claim'lere eklememektir örneğin şifre

    private readonly UserManager<AppUser> _userManager;

    public UserClaimProvider(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identityUser = principal.Identity as ClaimsIdentity; //Bu Identity claim identity sınıfından olacak.Kullanıcıyı tespit ederken elimizdeki cookie'lerin claim'den oluştuğunu bildirir.
        var curretUser = await _userManager.FindByNameAsync(identityUser.Name); //UserName gelir.

        if (curretUser is null) return principal;

        if (curretUser.City is null) return principal;

        if (principal.HasClaim(x => x.Type != "City"))
        {
            Claim cityClaim = new Claim("City", curretUser.City);
            //Claim nesnesini veritabanına kaydetmedik sadece cookie oluşmasını istiyoruz.Bu data zaten User tablosunda var tekrar claim tablosunda duplicate etmek istemiyoruz.
            identityUser.AddClaim(cityClaim);
        }
        return principal;
        // ! Yukarıdaki yöntem sayfa refresh edildiğinde veya başka sayfalara geçtiğimizde çalışıyor.Bu best practice ve performanslı değildir. Her seferinde claim'e eklemenin önüne geçeceğiz.

        //Bu kısmı Login action'da yapacağız.
      

    }
}