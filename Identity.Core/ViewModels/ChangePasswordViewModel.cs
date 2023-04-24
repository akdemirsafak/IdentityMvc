using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.Core.ViewModels;

public class ChangePasswordViewModel
{
    [DataType(DataType.Password)]
    [DisplayName("Eski Parola :  ")]
    [Required(ErrorMessage = "Eski Parola boş geçilemez.")]
    [MinLength(6, ErrorMessage = "Parola en az 6 karakter olabilir.")]
    public string OldPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [DisplayName("Yeni parola : ")]
    [Required(ErrorMessage = "Parola boş geçilemez.")]
    [MinLength(6, ErrorMessage = "Parola en az 6 karakter olabilir.")]
    public string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [DisplayName("Yeni Parola Tekrar : ")]
    [Required(ErrorMessage = "Parola boş bırakılamaz.")]
    [MinLength(6, ErrorMessage = "Parola en az 6 karakter olabilir.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Girilen parolalar eşleşmiyor.")]
    public string NewPasswordConfirm { get; set; } = null!;
}