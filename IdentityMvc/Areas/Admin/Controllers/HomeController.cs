using Identity.Repository.Models;
using IdentityMvc.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;

    private readonly UserManager<AppUser> _userManager;

    public HomeController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var userListViewModel = users.Select(x => new UserViewModel
        {
            Id = x.Id,
            UserName = x.UserName!,
            Email = x.Email!
        }).ToList();
        return View(userListViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var result = await _userManager.DeleteAsync(user!);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Kullanıcı silindi";
        }
        else
        {
            TempData["SuccessMessage"] = "Kullanıcı silinemedi.";
        }
        return RedirectToAction(nameof(Users));
    }


    public async Task<IActionResult> ResetUserPassword(string userId)
    {
        AppUser user= await _userManager.FindByIdAsync(userId);
        var passwordResetByAdminViewModel=new PasswordResetByAdminViewModel()
        {
            UserId= user.Id
        };
        return View(passwordResetByAdminViewModel);
    }
    [HttpPost]
    public async Task<IActionResult> ResetUserPassword(PasswordResetByAdminViewModel passwordResetByAdminViewModel)
    {
        AppUser user= (await _userManager.FindByIdAsync(passwordResetByAdminViewModel.UserId))!;
        //Kullanıcıya token göndermiyoruz.Kendimize token üretip işlemi tamamlıyoruz.
        var token= await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult=await _userManager.ResetPasswordAsync(user,token, passwordResetByAdminViewModel.NewPassword);
        await _userManager.UpdateSecurityStampAsync(user);//Kritik bir bilgi değiştiği için kullanıcının security stamp değerini de değiştiriyoruz.
                                                          //Bu sayede cookie'deki ile database'deki SecurityStamp değerleri uyuşmadığı için kullanıcının ekrar giriş yapması gerekecek.

        return RedirectToAction(nameof(Users));
    }
}