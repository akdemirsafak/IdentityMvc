using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityMvc.ViewModels;

public class ForgetPasswordViewModel
{
    [DisplayName("Email : ")]
    [Required(ErrorMessage = "Email geçilemez.")]
    public string Email { get; set; }
}