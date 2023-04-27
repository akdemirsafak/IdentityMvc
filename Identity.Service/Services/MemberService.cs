using Identity.Core.Models;
using Identity.Core.ViewModels;
using Identity.Repository.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Identity.Service.Services
{
    public class MemberService : IMemberService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IFileProvider _fileProvider;

        public MemberService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IFileProvider fileProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileProvider = fileProvider;
        }

        public async Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            var currentUser= (await _userManager.FindByNameAsync(userName))!;
            var resultChangePassword =await _userManager.ChangePasswordAsync(currentUser!, oldPassword, newPassword);
            if (!resultChangePassword.Succeeded)
            {
                return (false, resultChangePassword.Errors);
            }
            await _userManager.UpdateSecurityStampAsync(currentUser); //kullanıcının hassas bilgisi değiştiği için çıkış yapılmasını sağlar.
            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(currentUser, newPassword, true, false);
  
            return (true, null); //IEnumerable null olabilir bir type olduğu için null dönmemize izin verdi.
        }

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);
            return await _userManager.CheckPasswordAsync(currentUser, password);
        }

        public async Task<UserViewModel> GetUserByNameAsync(string userName)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);

            var userViewModel = currentUser.Adapt<UserViewModel>();
            userViewModel.PictureUrl = currentUser.Picture; //PictureUrl ve Picture isimlendirmesinden maplenmesi için özelleştirme gerekli.


            return userViewModel;
        }

        public async Task<UserEditViewModel> GetUserEditViewModelAsync(string userName)
        {
            var currentUser = (await _userManager.FindByNameAsync(userName))!;
            //var currentEditUserViewModel= currentUser.Adapt<UserEditViewModel>();
            var currentEditUserViewModel = new UserEditViewModel
            {
                UserName = currentUser!.UserName!,
                Email = currentUser!.Email!,
                PhoneNumber = currentUser.PhoneNumber,
                BirthDate = currentUser.BirthDate,
                City = currentUser.City,
                Gender = currentUser.Gender
            };
            return currentEditUserViewModel;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public SelectList GetGenderSelectList()
        {
            return new SelectList(Enum.GetNames(typeof(Gender)));   
        }

        public async Task<(bool, IEnumerable<IdentityError>?)> EditUserAsync(string userName, UserEditViewModel model)
        {
        
            var currentUser = (await (_userManager.FindByNameAsync(userName)))!;
            //currentUser = model.Adapt<AppUser>();
            currentUser.UserName = model.UserName;
            currentUser.Email = model.Email;
            currentUser.BirthDate = model.BirthDate;
            currentUser.City = model.City;
            currentUser.Gender = model.Gender;
            currentUser.PhoneNumber = model.PhoneNumber;
            if (model.Picture is not null && model.Picture.Length > 0)
            {
                var wwwRootFolder = _fileProvider.GetDirectoryContents("wwwroot");
                //Burda random bir dosya adı verelim.
                var randomFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(model.Picture.FileName)}";
                var newPicture = Path.Combine(wwwRootFolder!.First(x => x.Name == "userPictures").PhysicalPath!, randomFileName);
                using var stream = new FileStream(newPicture, FileMode.Create);
                await model.Picture.CopyToAsync(stream);
                //Görsel kaydedilirken klasör isimleriyle birlikte kaydetmek ileride yapılacak değişikliklerde sıkıntı yaratacağı için db ye sadece dosya adı yazılmalı. 
                currentUser.Picture = randomFileName;
            }

            var updateToUserResult = await _userManager.UpdateAsync(currentUser);
            if (!updateToUserResult.Succeeded)
            {
                return (false,updateToUserResult.Errors);
            }

            await _userManager.UpdateSecurityStampAsync(currentUser);
            await _signInManager.SignOutAsync();

            if (currentUser.BirthDate.HasValue)
            {
                await _signInManager.SignInWithClaimsAsync(currentUser, true, new[] { new Claim("BirthDate", currentUser.BirthDate.Value.ToString()) });
            }
            else
            {
                await _signInManager.SignInAsync(currentUser, true);
            }
            //-----------
            return (true, null);
          
        }

        public List<ClaimViewModel> GetClaims(ClaimsPrincipal claimsPrincipal)
        {
            
            return claimsPrincipal.Claims.Select(x => new ClaimViewModel()
            {
                Issuer = x.Issuer,
                Type = x.Type,
                Value = x.Value
            }).ToList();
        }
    }
}
