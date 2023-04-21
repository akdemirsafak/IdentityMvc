using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityMvc.ViewModels;

public class ResetPasswordViewModel
{
    [DataType(DataType.Password)]
    [DisplayName("Yeni parola : ")]
    [Required(ErrorMessage = "Parola boş geçilemez.")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [DisplayName("Yeni Parola Tekrar : ")]
    [Required(ErrorMessage = "Parola boş bırakılamaz.")]
    [Compare(nameof(Password), ErrorMessage = "Girilen parolalar eşleşmiyor.")]
    public string PasswordConfirm { get; set; }
}