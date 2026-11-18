using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using System.Net;
using System.Text.RegularExpressions;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inbox()
        {
            var userId = _userManager.GetUserId(User);
            var relatedUser = await _userManager.FindByIdAsync(userId);
            // Inbox
            var values = await _context.Messages
                .AsNoTracking()
                .Where(x => x.ReceiverEmail == relatedUser.Email && !x.DeletedByReceiver)
                .ToListAsync();

            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> CreateMessage(int? replyTo)
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");

            var model = new Message();

            if (replyTo.HasValue)
            {
                var user = await _userManager.GetUserAsync(User);

                var original = await _context.Messages.FirstOrDefaultAsync(x =>
                    x.MessageId == replyTo.Value &&
                    x.ReceiverEmail == user.Email &&
                    !x.DeletedByReceiver);

                if (original != null)
                {
                    model.ReceiverEmail = original.SenderEmail;

                    var subject = original.Subject ?? "";
                    model.Subject = subject.StartsWith("Yan:", StringComparison.OrdinalIgnoreCase)
                        ? subject
                        : "Yan: " + subject;

                    model.CategoryId = original.CategoryId;

                    ViewBag.Thread = await GetThreadAsync(original.MessageId, user.Email);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMessage(Message message, int? replyTo)
        {
            var user = await _userManager.GetUserAsync(User);

            message.MessageDetail = ToPlainText(message.MessageDetail);

            if (string.IsNullOrWhiteSpace(message.ReceiverEmail) ||
                string.IsNullOrWhiteSpace(message.Subject) ||
                string.IsNullOrWhiteSpace(message.MessageDetail))
            {
                ModelState.AddModelError("", "Alıcı, konu ve mesaj alanları boş bırakılamaz.");
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
                ViewBag.Thread = await GetThreadAsync(replyTo, user.Email);
                return View(message);
            }

            message.MessageId = 0;
            message.SenderEmail = user.Email;
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            message.IsFavourite = false;
            message.ParentMessageId = null;

            if (replyTo.HasValue)
            {
                var original = await _context.Messages.FirstOrDefaultAsync(x =>
                    x.MessageId == replyTo.Value &&
                    x.ReceiverEmail == user.Email &&
                    !x.DeletedByReceiver);

                if (original != null)
                {
                    message.ParentMessageId = original.MessageId;
                    message.ReceiverEmail = original.SenderEmail;   // yanıtın alıcısı hep orijinal gönderen
                }
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return RedirectToAction("SendBox");
        }


        public async Task<IActionResult> SendBox()
        {
            var user = await _userManager.GetUserAsync(User);

            var values = await _context.Messages
                    .AsNoTracking()
                    .Where(x => x.SenderEmail == user.Email && !x.DeletedBySender)
                    .OrderByDescending(x => x.SendDate)
                    .ToListAsync();

            return View(values);
        }

        [HttpGet("Message/Inbox/{id}")]
        public async Task<IActionResult> InboxDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var message = await _context.Messages
                .FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email && !x.DeletedByReceiver);

            if (message == null)
                return NotFound();

            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.Thread = await GetThreadAsync(message.ParentMessageId, user.Email);
            return View(message);
        }

        [HttpGet("Message/Sent/{id}")]
        public async Task<IActionResult> SentDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var message = await _context.Messages
                .FirstOrDefaultAsync(x => x.MessageId == id && x.SenderEmail == user.Email && !x.DeletedBySender);

            if (message == null)
                return NotFound();

            ViewBag.Thread = await GetThreadAsync(message.ParentMessageId, user.Email);
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStar(int id, bool? starred)
        {
            var user = await _userManager.GetUserAsync(User);

            var message = await _context.Messages
            .FirstOrDefaultAsync(x => x.MessageId == id &&
                              ((x.ReceiverEmail == user.Email && !x.DeletedByReceiver) ||
                               (x.SenderEmail == user.Email && !x.DeletedBySender)));

            if (message == null)
                return NotFound();

            // Liste sayfası starred değerini gönderir, detay sayfası göndermez (o zaman tersine çevir)
            message.IsFavourite = starred ?? !message.IsFavourite;
            await _context.SaveChangesAsync();

            // Liste sayfasındaki fetch isteği ise sadece OK dön
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok();

            return message.ReceiverEmail == user.Email
                ? RedirectToAction(nameof(InboxDetail), new { id })
                : RedirectToAction(nameof(SentDetail), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsUnread(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var message = await _context.Messages
                .FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email && !x.DeletedByReceiver);

            if (message == null)
                return NotFound();

            message.IsRead = false;
            await _context.SaveChangesAsync();

            // Detaya dönersek InboxDetail tekrar okundu yapar, o yüzden listeye dön
            return RedirectToAction(nameof(Inbox));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var message = await _context.Messages.FirstOrDefaultAsync(x =>
                x.MessageId == id &&
                (x.ReceiverEmail == user.Email || x.SenderEmail == user.Email));

            if (message == null)
                return NotFound();

            var isReceiver = message.ReceiverEmail == user.Email;
            var isSender = message.SenderEmail == user.Email;

            if (isReceiver) message.DeletedByReceiver = true;
            if (isSender) message.DeletedBySender = true;


            if (message.DeletedBySender && message.DeletedByReceiver)
                _context.Messages.Remove(message);

            await _context.SaveChangesAsync();

            return isReceiver && !isSender
                ? RedirectToAction(nameof(Inbox))
                : RedirectToAction(nameof(SendBox));
        }

        // Verilen mesajdan başlayıp yukarı doğru zinciri toplar (en yeni → en eski)
        private async Task<List<Message>> GetThreadAsync(int? startId, string email)
        {
            var list = new List<Message>();
            var visited = new HashSet<int>();
            var currentId = startId;

            while (currentId.HasValue && visited.Add(currentId.Value))
            {
                var id = currentId.Value;
                var m = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(x =>
                    x.MessageId == id &&
                    (x.ReceiverEmail == email || x.SenderEmail == email));

                if (m == null) break;

                list.Add(m);
                currentId = m.ParentMessageId;
            }

            return list;
        }

        private static string ToPlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";

            html = Regex.Replace(html, @"</(p|div|h[1-6]|li|blockquote)>|<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            var text = Regex.Replace(html, "<[^>]+>", "");
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            return text.Trim();
        }
    }
}
