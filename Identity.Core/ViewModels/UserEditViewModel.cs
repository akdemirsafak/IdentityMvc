using Identity.Core.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.Core.ViewModels;

public class UserEditViewModel
{
    [DisplayName("Kullanıcı Adı : ")]
    [Required(ErrorMessage = "Kullanıcı adı boş geçilemez.")]
    public string UserName { get; set; } = null!;

    [DisplayName("Email : ")]
    [Required(ErrorMessage = "Email geçilemez.")]
    public string Email { get; set; } = null!;

    [DisplayName("Telefon Numarası : ")]
    [Required(ErrorMessage = "Telefon numarası boş geçilemez.")]
    [RegularExpression(@"^(0(\d{3}) (\d{3}) (\d{2}) (\d{2}))$", ErrorMessage = "Telefon numarası formatı 0555 555 55 55 şeklinde olmalıdır.")]
    public string PhoneNumber { get; set; } = null!;

    [DisplayName("Doğum Tarihi : ")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [DisplayName("Şehir : ")]
    public string? City { get; set; }
    [DisplayName("Profil Resmi : ")]
    public IFormFile? Picture { get; set; }

    [DisplayName("Cinsiyet : ")]
    public Gender? Gender { get; set; }
}
