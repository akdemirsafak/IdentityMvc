using IdentityMvc.CustomValidations;
using IdentityMvc.Localizations;
using IdentityMvc.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityMvc.Extensions;

public static class StartupExtensions
{
    public static void AddIdentityWithExtention(this IServiceCollection serviceCollection)
    {
        serviceCollection.Configure<DataProtectionTokenProviderOptions>(
            options => { options.TokenLifespan = TimeSpan.FromHours(2); });

        //Gmail üzerinden ücretsiz mail gönderebiliriz. Google=>Manage Google Account =>Security =>Add Password(Uygulama şifreleri) yeni şifre oluşturuyoruz. 
        serviceCollection.AddIdentity<AppUser, AppRole>(
                options =>
                {
                    //Default ayarları değiştirmek istersek bu kısımda ayarlamalar yapabiliriz.
                    options.User.RequireUniqueEmail = true; // UserName default olarak unique !
                    options.User.AllowedUserNameCharacters =
                        "qwertyuopasdfghjklizxcvbnm1234567890_"; //sadece küçük harfler, sayılar ve _

                    //Parola ile ilgili düzenlemeler.
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false; //özel karakter zorunluluğunu kaldırdık
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireDigit = false;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(3); //Default olarak 5 dakika kitleniyormuş.
                    options.Lockout.MaxFailedAccessAttempts = 3; //Default 5 yanlış girişi 3 e düşürdük.
                })
            .AddUserValidator<UserValidator>()
            .AddPasswordValidator<PasswordValidator>()
            .AddErrorDescriber<LocalizationIdentityErrorDescriber>()
            .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
        //.AddTokenProvider(); ile kendi token'larımızı oluşturabiliriz.
    }
}