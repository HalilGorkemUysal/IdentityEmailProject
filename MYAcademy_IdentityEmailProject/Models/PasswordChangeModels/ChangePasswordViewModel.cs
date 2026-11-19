using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models.PasswordChangeModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gerekli.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre gerekli.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı gerekli.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}
