using System.ComponentModel.DataAnnotations;

namespace MYAcademy_IdentityEmailProject.Models.ProfileViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Ad boş bırakılamaz.")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad boş bırakılamaz.")]
        [StringLength(50)]
        public string Surname { get; set; }

        [StringLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
        public string? Description { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        public string? PhoneNumber { get; set; }

        // Sadece gösterim için
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ImageUrl { get; set; }

        public int InboxCount { get; set; }
        public int SentCount { get; set; }
        public int StarredCount { get; set; }
    }
}
