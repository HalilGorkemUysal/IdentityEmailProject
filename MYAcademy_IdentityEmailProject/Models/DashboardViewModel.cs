namespace MYAcademy_IdentityEmailProject.Models
{
    public class DashboardViewModel
    {
        // --- İstatistik kutuları ---
        public int TotalReceived { get; set; }      // Gelen kutusundaki toplam mesaj
        public int TotalSent { get; set; }          // Gönderilenlerdeki toplam mesaj
        public int UnreadCount { get; set; }        // Okunmamış gelen mesaj

        public int Days { get; set; }               // Kaç günlük dönem (5)
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int ReceivedLastDays { get; set; }   // Son 5 günde alınan
        public int SentLastDays { get; set; }       // Son 5 günde gönderilen
        public int ReceivedPrevDays { get; set; }   // Ondan önceki 5 günde alınan (karşılaştırma için)
        public int SentPrevDays { get; set; }       // Ondan önceki 5 günde gönderilen

        // --- Sütun grafik: her gün için bir değer ---
        public List<string> DayLabels { get; set; } = new();
        public List<int> DailyReceived { get; set; } = new();
        public List<int> DailySent { get; set; } = new();

        // --- Halka grafik: gelen mesajların kategoriye göre dağılımı ---
        public List<CategorySlice> Categories { get; set; } = new();
    }

}
