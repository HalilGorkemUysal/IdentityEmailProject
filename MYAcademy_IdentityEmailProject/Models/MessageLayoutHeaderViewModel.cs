namespace MYAcademy_IdentityEmailProject.Models
{
    // Navbar'da gösterilecek her şey
    public class MessageLayoutHeaderViewModel
    {
        public string FullName { get; set; } = "";
        public string Initials { get; set; } = "";   // "Ahmet Yılmaz" -> "AY"
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";   // AspNetRoles'tan, yoksa "Standart Hesap"

        public int UnreadCount { get; set; }
        public List<HeaderNotificationItem> Notifications { get; set; } = new();
    }
}
