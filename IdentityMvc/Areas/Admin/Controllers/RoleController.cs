using Identity.Repository.Models;
using IdentityMvc.Areas.Admin.ViewModels;
using IdentityMvc.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMvc.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public RoleController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.Select(x => new RoleViewModel
        {
            Id = x.Id,
            Name = x.Name!
        }).ToListAsync();
        return View(roles);
    }

    [Authorize(Roles = "role-action")]
    public IActionResult RoleCreate()
    {
        return View();
    }

    [Authorize(Roles = "role-action")]
    [HttpPost]
    public async Task<IActionResult> RoleCreate(RoleCreateViewModel roleCreateViewModel)
    {
        var result = await _roleManager.CreateAsync(new AppRole { Name = roleCreateViewModel.Name });

        if (!result.Succeeded)
        {
            ModelState.AddModelErrorList(result.Errors);
            return View();
        }

        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = "role-action")]
    public async Task<IActionResult> RoleUpdate(string id)
    {
        var roleToUpdate = await _roleManager.FindByIdAsync(id);
        if (roleToUpdate is null) throw new Exception("Güncellenecek Rol bulunamamıştır");
        var roleUpdateViewModel = roleToUpdate.Adapt<RoleUpdateViewModel>();
        return View(roleUpdateViewModel); //AppRole u RoleUpdateViewModel e mapledik.
    }
    [Authorize(Roles = "role-action")]
    [HttpPost]
    public async Task<IActionResult> RoleUpdate(RoleUpdateViewModel roleUpdateViewModel)
    {
        var role = await _roleManager.FindByIdAsync(roleUpdateViewModel.Id);
        //AppRole role = model.Adapt<AppRole>(); //model i AppRole e çevirip UpdateAsync e yollayalım.
        if (role is null)
        {
            ModelState.AddModelError(string.Empty, "Güncelleme işlemi başarısız oldu.");
            return View(roleUpdateViewModel);
        }

        role.Name = roleUpdateViewModel.Name;
        var result = await _roleManager.UpdateAsync(role);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Rol başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError(string.Empty, "Rol Güncellenemedi.");
        return View(roleUpdateViewModel);
    }
    [Authorize(Roles = "role-action")]
    public async Task<IActionResult> RoleDelete(string id)
    {
        var roleToDelete = _roleManager.FindByIdAsync(id).Result;
        if (roleToDelete is null)
        {
            //ModelState.AddModelError(String.Empty, "Rol bulunamamıştır.");
            TempData["SuccessMessage"] = "Rol bulunamamıştır.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(roleToDelete);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Rol başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Rol silinemedi.";
        //ModelState.AddModelError(String.Empty, "Rol Silinemedi.");
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = "role-action")]
    public async Task<IActionResult> AssignRoleToUser(string userId)
    {
        var currentUser = (await _userManager.FindByIdAsync(userId))!;
        ViewBag.userId = userId;
        var roles = await _roleManager.Roles.ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(currentUser);
        var roleViewModelList = new List<AssignRoleToUserViewModel>();
        foreach (var role in roles)
        {
            var assignRoleToUserViewModel = new AssignRoleToUserViewModel
            {
                Id = role.Id,
                Name = role.Name!
            };
            if (userRoles.Contains(role.Name!)) assignRoleToUserViewModel.Exist = true;
            roleViewModelList.Add(assignRoleToUserViewModel);
        }

        return View(roleViewModelList);
    }

    [HttpPost]
    [Authorize(Roles = "role-action")]
    public async Task<IActionResult> AssignRoleToUser(string userId, List<AssignRoleToUserViewModel> roleAssignListViewModel)
    {
        // var userId = TempData["userId"].ToString();
        // var user =await  _userManager.FindByIdAsync(userId);
        var userToAssignRoles = (await _userManager.FindByIdAsync(userId))!;
        foreach (var role in roleAssignListViewModel)
        {
            if (role.Exist)
            {
                await _userManager.AddToRoleAsync(userToAssignRoles, role.Name);
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(userToAssignRoles, role.Name);
            }
        }
        return RedirectToAction(nameof(HomeController.Users), "Home");
    }
}