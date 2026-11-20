using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class UserActivationViewModel
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Doğrulama kodu boş geçilemez.")]
        [Range(100000, 999999, ErrorMessage = "Doğrulama kodu 6 haneli olmalıdır.")]
        public int? Code { get; set; }
    }
}
