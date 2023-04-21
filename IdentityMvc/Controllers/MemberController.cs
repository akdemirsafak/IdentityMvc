using IdentityMvc.Extensions;
using IdentityMvc.Models;
using IdentityMvc.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;

namespace IdentityMvc.Controllers;

[Authorize]
public class MemberController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileProvider _fileProvider;

    public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IFileProvider fileProvider)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _fileProvider = fileProvider;
    }

    public async Task<IActionResult> Index()
    {
         var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);
    
        var userViewModel = currentUser.Adapt<UserViewModel>();
        userViewModel.PictureUrl = currentUser.Picture;
        return View(userViewModel);
    }
    // public async Task<IActionResult> LogOut()
    // {
    //     await _signInManager.SignOutAsync();
    //     return RedirectToAction("Index", "Home");
    // }

    public async Task Logout() //Daha efektif yöntemi
    {
        await _signInManager.SignOutAsync();
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
        var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

        var checkOldPassword = await _userManager.CheckPasswordAsync(currentUser, model.OldPassword);
        if (!checkOldPassword)
        {
            ModelState.AddModelError(string.Empty, "Eski şifre yanlış");
            return View();
        }

        var resultChangePassword =
            await _userManager.ChangePasswordAsync(currentUser, model.OldPassword, model.NewPassword);
        if (!resultChangePassword.Succeeded)
            ModelState.AddModelErrorList(resultChangePassword.Errors);

        await _userManager
            .UpdateSecurityStampAsync(
                currentUser); //kullanıcının hassas bilgisi değiştiği için çıkış yapılmasını sağlar.
        await _signInManager.SignOutAsync();
        await _signInManager.PasswordSignInAsync(currentUser, model.NewPassword, true, false);
        TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
        return View();
    }

    public async Task<IActionResult> UserEdit()
    {
        ViewBag.genderList = new SelectList(Enum.GetNames(typeof(Gender)));
        var currentUser = await (_userManager.FindByNameAsync(User.Identity!.Name!))!;
        var currentUserViewModel = new UserEditViewModel
        {
            UserName = currentUser!.UserName!,
            Email = currentUser!.Email!,
            PhoneNumber = currentUser.PhoneNumber,
            BirthDate = currentUser.BirthDate,
            City = currentUser.City,
            Gender = currentUser.Gender
        };
        return View(currentUserViewModel);
    }
    [HttpPost]
    public async Task<IActionResult> UserEdit(UserEditViewModel model)
    {
        if(!ModelState.IsValid) return View(model);
        var currentUser = await (_userManager.FindByNameAsync(User.Identity!.Name!));
        currentUser.UserName = model.UserName;
        currentUser.Email = model.Email;
        currentUser.BirthDate = model.BirthDate;
        currentUser.City = model.City;
        currentUser.Gender = model.Gender;
        currentUser.PhoneNumber= model.PhoneNumber;
        if (model.Picture is not null && model.Picture.Length>0)
        {
            var wwwRootFolder = _fileProvider.GetDirectoryContents("wwwroot");
            //Burda random bir dosya adı verelim.
            var randomFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(model.Picture.FileName)}";
            var newPicture = Path.Combine(wwwRootFolder!.First(x=>x.Name=="userPictures").PhysicalPath!, randomFileName);
            using var stream = new FileStream(newPicture, FileMode.Create);
            await model.Picture.CopyToAsync(stream);
            //Görsel kaydedilirken klasör isimleriyle birlikte kaydetmek ileride yapılacak değişikliklerde sıkıntı yaratacağı için db ye sadece dosya adı yazılmalı. 
            currentUser.Picture = randomFileName;
        }
        
        var updateToUserResult = await _userManager.UpdateAsync(currentUser);
        if (!updateToUserResult.Succeeded)
        {
            ModelState.AddModelErrorList(updateToUserResult.Errors);
            return View();
        }
        await _userManager.UpdateSecurityStampAsync(currentUser);
        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(currentUser, true);
        TempData["SuccessMessage"] = "Üye bilgileri başarıyla değiştirilmiştir.";
        // var userEditViewModel = new UserEditViewModel()
        // {
        //     UserName = currentUser.UserName,
        //     Email = currentUser.Email,
        //     BirthDate = currentUser.BirthDate,
        //     City = currentUser.City,
        //     Gender = currentUser.Gender,
        //     PhoneNumber= currentUser.PhoneNumber
        //     //Buraya picture da gelecek geldiğinde mapster ile mapleyeceğim.
        //     
        // };
        var userViewModel = new UserViewModel()
        {
            UserName = currentUser.UserName,
            Email = currentUser.Email,
            City = currentUser.City,
            Gender = currentUser.Gender,
            PhoneNumber = currentUser.PhoneNumber
        };
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
        string message=string.Empty;
        message = "Bu sayfayı görmeye yetkiniz yoktur.";
        ViewBag.message = message;
        
        return View();
    }

    [HttpGet]
    public IActionResult Claims()
    {
        var userClaimsList = User.Claims.Select(x => new ClaimViewModel()
        {
            Issuer = x.Issuer,
            Type = x.Type,
            Value = x.Value
        }).ToList();
        return View(userClaimsList);
    }

    [Authorize(Policy = "AnkaraPolicy")]
    [HttpGet]
    public IActionResult AnkaraPage()
    {
        return View();
    }
}