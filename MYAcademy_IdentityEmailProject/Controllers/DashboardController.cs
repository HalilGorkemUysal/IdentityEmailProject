using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using System.Globalization;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class DashboardController : Controller
    {
        private const int Days = 5; // "Son 5 gün". Değiştirirseniz kutular ve grafik birlikte değişir.

        // Kategori renkleri. Sidebar'daki etiket noktalarıyla aynı sırada kullanılır
        // (kategoriler ada göre sıralı). 5'ten fazla kategori "Diğer" diliminde toplanır.
        public static readonly string[] CategoryPalette = { "#8B7FD6", "#3E9A77", "#C8774A", "#4A7CC2", "#C2476B" };
        public const string OtherColor = "#A3A6B8";

        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("SignIn", "Login");

            var email = user.Email!;

            // Inbox ve SendBox sayfalarıyla AYNI kurallar → sidebar rozetleriyle sayılar tutar
            var received = _context.Messages.AsNoTracking()
                .Where(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft);

            var sent = _context.Messages.AsNoTracking()
                .Where(x => x.SenderEmail == email && !x.DeletedBySender && !x.IsDraft);

            var today = DateTime.Today;
            var from = today.AddDays(-(Days - 1));   // bugün dahil 5 gün: 16, 17, 18, 19, 20
            var prevFrom = from.AddDays(-Days);      // ondan önceki 5 gün: 11 … 15

            var model = new DashboardViewModel
            {
                Days = Days,
                FromDate = from,
                ToDate = today,

                TotalReceived = await received.CountAsync(),
                TotalSent = await sent.CountAsync(),
                UnreadCount = await received.CountAsync(x => !x.IsRead),

                ReceivedLastDays = await received.CountAsync(x => x.SendDate >= from),
                SentLastDays = await sent.CountAsync(x => x.SendDate >= from),

                ReceivedPrevDays = await received.CountAsync(x => x.SendDate >= prevFrom && x.SendDate < from),
                SentPrevDays = await sent.CountAsync(x => x.SendDate >= prevFrom && x.SendDate < from)
            };

            // --- Sütun grafik: gün gün sayılar ---
            // Gruplama veritabanında yapılır (SQL: GROUP BY CAST(SendDate AS date))
            var receivedByDay = await received
                .Where(x => x.SendDate >= from)
                .GroupBy(x => x.SendDate.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Day, x => x.Count);

            var sentByDay = await sent
                .Where(x => x.SendDate >= from)
                .GroupBy(x => x.SendDate.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Day, x => x.Count);

            // Hiç mesaj olmayan günler sözlükte yoktur, 0 yazıyoruz ki grafikte o gün boş kalmasın
            var tr = new CultureInfo("tr-TR");
            for (var i = 0; i < Days; i++)
            {
                var day = from.AddDays(i);
                model.DayLabels.Add(day == today ? "Bugün" : day.ToString("ddd, d MMM", tr));
                model.DailyReceived.Add(receivedByDay.GetValueOrDefault(day));
                model.DailySent.Add(sentByDay.GetValueOrDefault(day));
            }

            // --- Halka grafik: gelen mesajların kategoriye göre dağılımı ---
            var categories = await _context.Categories.AsNoTracking()
                .OrderBy(c => c.CategoryName)            // sidebar ile aynı sıra → aynı renk
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            var countsByCategory = await received
                .GroupBy(x => x.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            var otherCount = 0;
            for (var i = 0; i < categories.Count; i++)
            {
                var count = countsByCategory.GetValueOrDefault(categories[i].CategoryId);
                if (count == 0) continue;   // boş kategori grafikte dilim olmasın (rengi yine sabit kalır)

                if (i < CategoryPalette.Length)
                {
                    model.Categories.Add(new CategorySlice
                    {
                        Name = categories[i].CategoryName,
                        Count = count,
                        Color = CategoryPalette[i]
                    });
                }
                else
                {
                    otherCount += count;
                }
            }

            if (otherCount > 0)
            {
                model.Categories.Add(new CategorySlice { Name = "Diğer", Count = otherCount, Color = OtherColor });
            }

            return View(model);
        }
    }
}
