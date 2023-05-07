using Identity.Core.Models;
using Identity.Core.ViewModels;
using Identity.Repository.Models;
using Identity.Service.Services;
using IdentityMvc.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityMvc.Controllers;

public class HomeController : Controller
{
    private readonly IEmailService _emailService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid)
            return View();
        if (_userManager.Users.Any(x => x.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError("", "Bu telefon numarası başka bir hesapta kullanılıyor.!");
            return View(model);
        }

        var identityResult = await _userManager.CreateAsync(new AppUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        }, model.Password!);

        if (!identityResult.Succeeded)
        {
            ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
            return View();
        }
        var exchangeExpireClaim = new Claim("ExchangeExpireData", DateTime.Now.AddDays(10).ToString()); //Burada 2. senaryo gereği kullanıcının kayıt olduğu günden itibaren 10 gün boyunca free kullanımı için claim oluşturuyoruz. 

        var user = await _userManager.FindByNameAsync(model.UserName);
        //kullanıcı mail'ini doğrulaması
        string confirmationEmailToken = (await _userManager.GenerateEmailConfirmationTokenAsync(user!))!;
        string link = (Url.Action("ConfirmEmail", "Home", new
        {
            userId = user.Id,
            token = confirmationEmailToken
        }, protocol: HttpContext.Request.Scheme))!;
        await _emailService.ConfirmEmailAsync(link, user.Email!);
        /// Kullanıcı mail doğrulaması bitti
        var claimResult = await _userManager.AddClaimAsync(user!, exchangeExpireClaim); //user tablosunda kayıt olduğu tarihi tutmadan UserClaim tablosu üzerinden işlem yaptık.

        if (!claimResult.Succeeded)
        {
            ModelState.AddModelErrorList(claimResult.Errors.Select(x => x.Description).ToList());
            return View();
        }

        TempData["SuccessMessage"] = "Üyelik işlemi başarıyla tamamlandı.";
        return RedirectToAction(nameof(SignUp));

    }

    public IActionResult Login()
    {
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View();

        returnUrl = returnUrl ?? Url.Action("Index", "Home");
        //var result = _signInManager.PasswordSignInAsync(model.UserName, model.Password, model); //Eğer username ila giriş yaptırıyorsak
        var hasUser = await _userManager.FindByEmailAsync(model.Email);
        if (hasUser == null)
        {
            ModelState.AddModelError(
                string.Empty, "Email veya şifre yanlış.");
            return View();
        }
        if (!_userManager.IsEmailConfirmedAsync(hasUser).Result)
        {
            ModelState.AddModelErrorList(new List<string> { "Hesabınızı doğrulamanız gerekli." });
            return View();
        }


        var loginResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);
        //buradaki false kullanıcı bilgilerinin uzun vadede cookie'de tutulması durumudur.
        //Kullanıcı n tane yanlış şifre girişi yaptığında hesabı kitlensin veya kitlenmesin.Default olarak 5 dir.


        if (loginResult.IsLockedOut)
        {
            ModelState.AddModelErrorList(new List<string> { "Hesabınıza 3 dakikalığına giriş yapamayacaksınız.." });
            return View();
        }

        if (!loginResult.Succeeded)
        {
            ModelState.AddModelErrorList(new List<string> { $"Email veya şifre yanlış.", $"Başarısız giriş sayısı : {await _userManager.GetAccessFailedCountAsync(hasUser)}" });
            return View();
        }

        if (hasUser.BirthDate.HasValue)
        {
            await _signInManager.SignInWithClaimsAsync(hasUser, model.RememberMe, new[] { new Claim("BirthDate", hasUser.BirthDate.Value.ToString()) });
        }
        return Redirect(returnUrl!);


    }

    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View();
        //link gönderilecek
        //https://localhost:7000?userid=xxxx&token=asdasd123 benzeri bir yapı oluşacak.
        //Burada username de gönderilebilir.Tercihimize bağlı. Token'ın önemi : Bu şifre belirleme email'inin süresini ayarlamak.

        var hasUser = await _userManager.FindByEmailAsync(model.Email);
        if (hasUser == null)
        {
            ModelState.AddModelError(string.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
            return View(); //Redirect yapılması mantıksal açıdan doğru değil çünkü ModelState ile data tutamayız.
        }

        var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);
        var passwordResetLink = Url.Action("ResetPassword", "Home",
            new
            {
                userId = hasUser.Id,
                token = passwordResetToken
            }, HttpContext.Request.Scheme);

        await _emailService.SendResetPasswordEmailAsync(passwordResetLink!, model.Email);

        TempData["SuccessMessage"] = "Parola belirleme linki e posta adresinize gönderilmiştir.";
        return RedirectToAction(nameof(ForgetPassword));
    }

    public IActionResult ResetPassword(string userId, string token)
    {
        //TempData[""] Request'ler arası data taşıma - Tek seferlik okuma sağlar.
        TempData["userId"] = userId;
        TempData["token"] = token;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        var userId = TempData["userId"];
        var token = TempData["token"];
        if (userId is null || token is null)
            throw new Exception("Bir hata oluştu.");
        var hasUser = await _userManager.FindByIdAsync(userId!.ToString());
        if (hasUser == null)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı bulunamamıştır.");
            return View();
        }

        var result = await _userManager.ResetPasswordAsync(hasUser, token!.ToString(), model.Password);
        if (result.Succeeded)
            TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
        else
            ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
        return RedirectToAction(nameof(Login));
    }


    [HttpGet]
    public IActionResult ConfirmEmail()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(string email)
    {

        if (!ModelState.IsValid)
            return View();

        var hasUser = await _userManager.FindByEmailAsync(email);
        if (hasUser == null)
        {
            ModelState.AddModelErrorList(new List<string> { "Bu email adresine sahip kullanıcı bulunamamıştır." });
            return View(); //Redirect yapılması mantıksal açıdan doğru değil çünkü ModelState ile data tutamayız.
        }
        if (hasUser.EmailConfirmed)
        {
            TempData["SuccessMessage"] = "Mail adresiniz zaten doğrulanmış.";
            return RedirectToAction(nameof(Login));
        }

        string confirmationEmailToken = (await _userManager.GenerateEmailConfirmationTokenAsync(hasUser))!;
        string link = (Url.Action("Login", "Home", new
        {
            userId = hasUser.Id,
            token = confirmationEmailToken
        }, protocol: HttpContext.Request.Scheme))!;

        await _emailService.ConfirmEmailAsync(link, email);

        TempData["SuccessMessage"] = "Mail doğrulama linki e posta adresinize gönderilmiştir.";

        return RedirectToAction(nameof(Login));
    }

    public IActionResult FacebookLogin(string returnUrl = null)
    {
        string redirectUrl = Url.Action("ExternalResponse", "Home", new
        {
            ReturnUrl = returnUrl
        }); //Kullanıcının facebook login işleminden sonra yönlendirileceği yer.
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
        return new ChallengeResult("Facebook", properties); //Ne verilirse kullanıcıyı oraya yönlendirir.Verdiğimiz parametrelerle ActionResult'tan kalıtım alır.
    }
    public IActionResult GoogleLogin(string returnUrl = null)
    {
        string redirectUrl = Url.Action("ExternalResponse", "Home", new
        {
            ReturnUrl = returnUrl
        }); //Kullanıcının facebook login işleminden sonra yönlendirileceği yer.
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return new ChallengeResult("Google", properties); //Ne verilirse kullanıcıyı oraya yönlendirir.Verdiğimiz parametrelerle ActionResult'tan kalıtım alır.
    }
    public IActionResult MicrosoftLogin(string returnUrl = null)
    {
        string redirectUrl = Url.Action("ExternalResponse", "Home", new
        {
            ReturnUrl = returnUrl
        }); //Kullanıcının microsoft login işleminden sonra yönlendirileceği yer.
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);
        return new ChallengeResult("Microsoft", properties); //Ne verilirse kullanıcıyı oraya yönlendirir.Verdiğimiz parametrelerle ActionResult'tan kalıtım alır.
    }


    public async Task<IActionResult> ExternalResponse(string returnUrl = "/")//3th party authentication'un döndüğü olay burada.
    {
        ExternalLoginInfo externalLoginInfo = (await _signInManager.GetExternalLoginInfoAsync())!; //Kullanıcının login olmasıyla ilgili bilgiler getirecek.
        //LoginProvider'da kullanıcı facebook'la login facebookId si gelir.

        if (externalLoginInfo == null)
            return RedirectToAction(nameof(Login));                  //yukarıda kullanıcıyı facebook login sayfasına gönderdik fakat kullanıcı bilgileri göndermezse kontrolü yapıyoruz.



        Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, true); //Buradaki true' cookie'de yaptığımız ayarların geçerli olup olmaması durumunu özetler.
        if (signInResult.Succeeded) //Daha önce bu yöntemle kayıt olunduysa db'de bu bilgiler succceded ile gelecek.
        {
            return Redirect(returnUrl);
        }
        //Kullanıcı ilk defa kayıt oluyorsa bu işlemler yapılacak
        else
        {
            AppUser user = new();

            user.Email = externalLoginInfo.Principal.FindFirst(ClaimTypes.Email)!.Value;
            string externalUserId = externalLoginInfo.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (externalLoginInfo.Principal.HasClaim(x => x.Type == ClaimTypes.Name))
            {
                string userName = externalLoginInfo.Principal.FindFirst(ClaimTypes.Name)!.Value;
                userName = userName.Replace(' ', '-').ToLower().Replace('ş', 's') + externalUserId.Substring(0, 5).ToString();
                //Username olarak direkt email adresi verilen siteler de var bu da farklı bir çözüm.
                user.UserName = userName;
            }
            else
            {
                user.UserName = externalLoginInfo.Principal.FindFirst(ClaimTypes.Email)!.Value;
            }
            //! xyz@hotmail.com mail adresiyle facebook hesabı oluşturmuş olabiliriz.Bu senaryoda facebookla veya microsoftla giriş yaptığımızda kullanıcı oluşacak fakat diğer yöntemle giriş yaptığımızda hata alacağız.Bunun önüne Aşağıdaki yöntemle geçeceğiz.

            AppUser existUser= await _userManager.FindByEmailAsync(user.Email);
            if (existUser != null) //Bu maille kullanıcı varsa
            {
                IdentityResult loginResult = await _userManager.AddLoginAsync(existUser, externalLoginInfo);
                if (loginResult.Succeeded)
                {
                    await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, true); // 3th party auth. olduğunu belirtmek için, ayrıca claim'e bu eklenir claim based authorization yapmamıza da imkanımız olur.
                    //await _signInManager.SignInAsync(user, true);
                    return Redirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelErrorList(loginResult.Errors);
                }
            }
            else
            {
                IdentityResult createUserResult = await _userManager.CreateAsync(user);
                if (createUserResult.Succeeded)
                {
                    IdentityResult loginResult = await _userManager.AddLoginAsync(user, externalLoginInfo);
                    if (loginResult.Succeeded)
                    {
                        await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, true); // 3th party auth. olduğunu belirtmek için, ayrıca claim'e bu eklenir claim based authorization yapmamıza da imkanımız olur.
                                                                                                                                             //await _signInManager.SignInAsync(user, true);
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelErrorList(loginResult.Errors);
                    }
                }
                else
                {
                    ModelState.AddModelErrorList(createUserResult.Errors);
                }
            }

        }
        List<string> errors = ModelState.Values.SelectMany(x=>x.Errors).Select(y=>y.ErrorMessage).ToList();


        //return RedirectToAction(nameof(ErrorPage));
        return View("ErrorPage", errors);
    }


    public IActionResult ErrorPage()
    {
        return View();
    }
}