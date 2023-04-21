using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityMvc.Areas.Admin.ViewModels;

public class RoleCreateViewModel
{
    [DisplayName("Rol Adı : ")]
    [Required(ErrorMessage = "Rol ismi gereklidir.")]
    public string Name { get; set; }
}