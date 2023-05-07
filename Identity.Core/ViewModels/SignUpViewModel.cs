using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.Core.ViewModels;

public class SignUpViewModel
{
    [DisplayName("Kullanıcı Adı : ")]
    [Required(ErrorMessage = "Kullanıcı adı boş geçilemez.")]
    public string UserName { get; set; } = null!;

    [DisplayName("Email : ")]
    [Required(ErrorMessage = "Email geçilemez.")]
    public string Email { get; set; } = null!;

    [DisplayName("Telefon Numarası : ")]
    [Required(ErrorMessage = "Telefon numarası boş geçilemez.")]
    //[RegularExpression(@"^(0(\\d{3}) (\\d{3}) (\\d{2}) (\\d{2}))$", ErrorMessage = "Formata uygun olmayan bir telefon numarası!")]
    [RegularExpression(@"^(0(\d{3}) (\d{3}) (\d{2}) (\d{2}))$", ErrorMessage = "Telefon numarası formatı 0555 555 55 55 şeklinde olmalıdır.")]
    public string PhoneNumber { get; set; } = null!;


    [DataType(DataType.Password)]
    [DisplayName("Parola : ")]
    [Required(ErrorMessage = "Parola boş geçilemez.")]
    public string Password { get; set; } = null!;


    [DataType(DataType.Password)]
    [DisplayName("Parola Tekrar : ")]
    [Required(ErrorMessage = "Parola boş bırakılamaz.")]
    [Compare(nameof(Password), ErrorMessage = "Girilen şifreler eşleşmiyor.")]
    public string PasswordConfirm { get; set; } = null!;
}