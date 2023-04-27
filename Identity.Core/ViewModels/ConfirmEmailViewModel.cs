using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.Core.ViewModels
{
    public class ConfirmEmailViewModel
    {
        [DisplayName("Email")]
        [Required(ErrorMessage = "Bu alan boş geçilemez.")]
        public required string Email { get; set; }
    }
}
