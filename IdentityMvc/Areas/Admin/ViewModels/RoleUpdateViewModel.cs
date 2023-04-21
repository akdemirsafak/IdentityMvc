using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityMvc.Areas.Admin.ViewModels;

public class RoleUpdateViewModel
{
    [DisplayName("Rol adı")]
    [Required(ErrorMessage = "Rol adı gereklidir.")]
    public string Name { get; set; }

    public string Id { get; set; }
}