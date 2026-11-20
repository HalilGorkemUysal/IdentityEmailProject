using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    public class MessageLayoutSidebarViewComponent : ViewComponent
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MessageLayoutSidebarViewComponent(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            if (user != null)
            {
                var email = user.Email;
                var messages = _context.Messages.AsNoTracking();

                // Inbox action'ı ile aynı kural
                ViewBag.InboxCount = await messages
                    .CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft);

                // SendBox action'ı ile aynı kural
                ViewBag.SentCount = await messages
                    .CountAsync(x => x.SenderEmail == email && !x.DeletedBySender && !x.IsDraft);

                // DraftsOf(email) ile aynı kural
                ViewBag.DraftCount = await messages
                    .CountAsync(x => x.SenderEmail == email && x.IsDraft);

                // Starred action'ı ile aynı kural
                ViewBag.StarredCount = await messages
                    .CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft && x.IsFavourite);

                // Çöp kutusu = TrashReceived + TrashSent (Trash sayfasındaki iki sekmenin toplamı)
                var trashReceived = await messages
                    .CountAsync(x => x.ReceiverEmail == email && x.DeletedByReceiver && !x.PurgedByReceiver);

                var trashSent = await messages
                    .CountAsync(x => x.SenderEmail == email && x.DeletedBySender && !x.PurgedBySender);

                ViewBag.TrashCount = trashReceived + trashSent;
            }

            return View(categories);
        }

    }
}
