using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityMvc.Models;

public class LoginViewModel
{
    [DisplayName("Kullanıcı Adı : ")]
    [Required(ErrorMessage = "Email boş geçilemez.")]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    [DisplayName("Şifre : ")]
    [Required(ErrorMessage = "Parola boş bırakılamaz.")]
    public string Password { get; set; }

    [DisplayName("Beni Hatırla")] public bool RememberMe { get; set; }
}