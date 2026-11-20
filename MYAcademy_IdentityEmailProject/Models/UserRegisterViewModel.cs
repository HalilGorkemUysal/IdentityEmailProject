using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage ="Ad alanı zorunludur.")]
        [StringLength(50,ErrorMessage ="Ad en fazla 50 karakter olabilir.")]
        [Display(Name="Ad")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string Surname { get; set; }

        [Required(ErrorMessage ="Kullanıcı adı zorunludur.")]
        [StringLength(30, MinimumLength =3, ErrorMessage ="Kullanıcı adı 3-30 karakter arası olmalıdır")]
        [Display(Name="Kullanıcı Adı")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name="Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [DataType(DataType.Password)]
        [MinLength(8,ErrorMessage ="Şifre en az 8 karakter olmalıdır.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage ="Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),ErrorMessage ="Şifreler uyuşmuyor")]
        [Display(Name = "Şifre Tekrar")]
        public string PasswordConfirm { get; set; }
    }
}
