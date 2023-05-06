using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityMvc.Areas.Admin.ViewModels
{
    public class PasswordResetByAdminViewModel
    {

        public string UserId { get; set; }
        [DataType(DataType.Password)]
        [DisplayName("Yeni Şifre : ")]
        public string NewPassword { get; set; }
    }
}
