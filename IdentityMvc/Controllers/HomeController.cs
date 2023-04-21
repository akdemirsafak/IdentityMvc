using IdentityMvc.Extensions;
using IdentityMvc.Models;
using IdentityMvc.Services;
using IdentityMvc.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        if (!ModelState.IsValid) return View();

        var identityResult = await _userManager.CreateAsync(new AppUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        }, model.Password!);

        if (identityResult.Succeeded)
        {
            TempData["SuccessMessage"] = "Üyelik işlemi başarıyla tamamlandı.";
            return RedirectToAction(nameof(SignUp));
        }

        ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        if (!ModelState.IsValid) return View();

        returnUrl = returnUrl ?? Url.Action("Index", "Home");
        //var result = _signInManager.PasswordSignInAsync(model.UserName, model.Password, model); //Eğer username ila giriş yaptırıyorsak
        var hasUser = await _userManager.FindByEmailAsync(model.Email);
        if (hasUser == null)
        {
            ModelState.AddModelError(
                string.Empty, "Email veya şifre yanlış.");
            return View();
        }

        var loginResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);
        //buradaki false kullanıcı bilgilerinin uzun vadede cookie'de tutulması durumudur.
        //Kullanıcı n tane yanlış şifre girişi yaptığında hesabı kitlensin veya kitlenmesin.Default olarak 5 dir.

        if (loginResult.Succeeded) return Redirect(returnUrl);
        if (loginResult.IsLockedOut)
        {
            ModelState.AddModelErrorList(new List<string> { "Hesabınız 3 dakikalığına kitlendi." });
            return View();
        }

        var lockoutCount = await _userManager.GetAccessFailedCountAsync(hasUser);
        ModelState.AddModelErrorList(new List<string> { $"Yanlış giriş sayısı : {lockoutCount}" });
        ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış." });
        return View();
    }

    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View();
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

        await _emailService.SendResetPasswordEmailAsync(passwordResetLink, model.Email);

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
        if (userId is null || token is null) throw new Exception("Bir hata oluştu.");
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
}