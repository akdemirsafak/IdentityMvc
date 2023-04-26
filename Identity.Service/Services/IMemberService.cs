using Identity.Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Identity.Service.Services
{
    public interface IMemberService
    {
        Task<UserViewModel> GetUserByNameAsync(string userName);
        Task LogoutAsync();
        Task<bool> CheckPasswordAsync(string userName, string password);
        Task<(bool,IEnumerable<IdentityError>?)> ChangePasswordAsync(string userName, string oldPassword,string newPassword);
        Task<UserEditViewModel> GetUserEditViewModelAsync(string userName);
        SelectList GetGenderSelectList();
        Task<(bool, IEnumerable<IdentityError>?)> EditUserAsync(string userName, UserEditViewModel model);
        List<ClaimViewModel> GetClaims(ClaimsPrincipal claimsPrincipal);
    }
}
