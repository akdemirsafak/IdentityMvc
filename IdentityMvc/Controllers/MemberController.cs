using Identity.Core.ViewModels;
using Identity.Service.Services;
using IdentityMvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Repository.Controllers;

[Authorize]
public class MemberController : Controller
{
    private string userName => User.Identity!.Name!; //Set'i olmayan sadece get'e sahip property
    private readonly IMemberService _memberService;

    public MemberController(IMemberService memberService)
    {
        _memberService = memberService;
    }
    public async Task<IActionResult> Index()
    {
        var userViewModel = await _memberService.GetUserByNameAsync(userName);
        return View(userViewModel);
    }
    // public async Task<IActionResult> LogOut()
    // {
    //     await _signInManager.SignOutAsync();
    //     return RedirectToAction("Index", "Home");
    // }

    public async Task Logout() //Daha efektif yöntemi
    {
        await _memberService.LogoutAsync();
    }

    //Diğer controller'larda sadece üyelerin erişmesini istediğimiz endpointlere [Authorize] attribute ü kullanmalıyız.

    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View();
        var checkOldPassword = await _memberService.CheckPasswordAsync(userName, model.OldPassword);
        if (!checkOldPassword)
        {
            ModelState.AddModelError(string.Empty, "Eski şifre yanlış");
            return View();
        }
        var (isSuccess, errors) = await _memberService.ChangePasswordAsync(userName, model.OldPassword, model.NewPassword);
        if (!isSuccess)
        {
            ModelState.AddModelErrorList(errors!);
        }


        TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
        return View();
    }

    public async Task<IActionResult> UserEdit()
    {
        ViewBag.genderList = _memberService.GetGenderSelectList();
        return View(await _memberService.GetUserEditViewModelAsync(userName));
    }
    [HttpPost]
    public async Task<IActionResult> UserEdit(UserEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (isSuccess, errors) = await _memberService.EditUserAsync(userName, model);
        if (!isSuccess)
        {
            ModelState.AddModelErrorList(errors!);
            return View();
        }
        TempData["SuccessMessage"] = "Üye bilgileri başarıyla değiştirilmiştir.";

        var userViewModel = await _memberService.GetUserEditViewModelAsync(userName);

        return RedirectToAction("Index", userViewModel);
    }

    [Authorize(Roles = "Editor")]
    public IActionResult Editor()
    {
        return View();
    }

    [Authorize(Roles = "Manager,Admin")]
    public IActionResult Manager()
    {
        return View();
    }


    public IActionResult AccessDenied(string returnUrl)
    {
        string message = string.Empty;
        message = "Bu sayfayı görmeye yetkiniz yoktur.";
        ViewBag.message = message;

        return View();
    }

    [HttpGet]
    public IActionResult Claims()
    {
        return View(_memberService.GetClaims(User));
    }

    [Authorize(Policy = "AnkaraPolicy")]
    [HttpGet]
    public IActionResult AnkaraPage()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = "ExchangePolicy")]
    public IActionResult ExchangePage()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = "ViolencePolicy")]
    public IActionResult ViolencePage()
    {
        return View();
    }
}