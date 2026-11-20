using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class UserLoginViewModel
    {
        [Required(ErrorMessage ="Kullanıcı adı boş geçilemez.")]
        [Display(Name ="Kullanıcı Adı")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre boş geçilemez.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
