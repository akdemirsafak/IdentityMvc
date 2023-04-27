using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Core.ViewModels
{
    public class ConfirmEmailViewModel
    {
        [DisplayName("Email")]
        [Required(ErrorMessage ="Bu alan boş geçilemez.")]
        public required string Email { get; set; }
    }
}
