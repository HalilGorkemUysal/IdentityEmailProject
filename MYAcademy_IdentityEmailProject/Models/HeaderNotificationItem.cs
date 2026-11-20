namespace MYAcademy_IdentityEmailProject.Models
{
    // Bildirim menüsündeki tek satır (okunmamış bir mesaj)
    public class HeaderNotificationItem
    {
        public string SenderEmail { get; set; } = "";
        public string Subject { get; set; } = "";
        public DateTime SendDate { get; set; }
    }
}
