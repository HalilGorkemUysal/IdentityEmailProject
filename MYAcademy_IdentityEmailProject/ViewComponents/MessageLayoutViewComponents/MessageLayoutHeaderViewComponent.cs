using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    public class MessageLayoutHeaderViewComponent : ViewComponent //Navbar yani burası
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailContext _emailContext;

        public MessageLayoutHeaderViewComponent(UserManager<AppUser> userManager, EmailContext emailContext)
        {
            _userManager = userManager;
            _emailContext = emailContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);

            var roles = await _userManager.GetRolesAsync(user);

            var unreadQuery = _emailContext.Messages
                .Where(m => m.ReceiverEmail == user.Email && !m.IsRead && !m.DeletedByReceiver);

            var unreadCount = await unreadQuery.CountAsync();

            var notifications = await unreadQuery
                .OrderByDescending(m => m.SendDate)
                .Take(5)
                .Select(m => new HeaderNotificationItem
                {
                    SenderEmail = m.SenderEmail,
                    Subject = m.Subject,
                    SendDate = m.SendDate
                })
                .ToListAsync();

            var model = new MessageLayoutHeaderViewModel
            {
                FullName = $"{user.Name} {user.Surname}".Trim(),
                Initials = GetInitials(user.Name, user.Surname),
                Email = user.Email ?? "",
                RoleName = roles.FirstOrDefault() ?? "Standart Hesap",
                UnreadCount = unreadCount,
                Notifications = notifications
            };

            return View(model);
        }

        private static string GetInitials(string? name, string? surname)
        {
            var first = string.IsNullOrWhiteSpace(name) ? "" : name.Trim()[..1];
            var last = string.IsNullOrWhiteSpace(surname) ? "" : surname.Trim()[..1];
            var initials = (first + last).ToUpper(new System.Globalization.CultureInfo("tr-TR"));
            return string.IsNullOrEmpty(initials) ? "?" : initials;
        }
    }
}
