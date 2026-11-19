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

                ViewBag.InboxCount = await _context.Messages.AsNoTracking()
                    .CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft);

                ViewBag.SentCount = await _context.Messages.AsNoTracking()
                    .CountAsync(x => x.SenderEmail == email && !x.DeletedBySender && !x.IsDraft);

                ViewBag.DraftCount = await _context.Messages.AsNoTracking()
                    .CountAsync(x => x.SenderEmail == email && x.IsDraft);

                ViewBag.StarredCount = await _context.Messages.AsNoTracking()
                    .CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft && x.IsFavourite);

                ViewBag.StarredCount = await _context.Messages.AsNoTracking()
                    .CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft && x.IsFavourite);
            }

            return View(categories);
        }

    }
}
