using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Context;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models.PasswordChangeModels;
using MYAcademy_IdentityEmailProject.Models.ProfileViewModels;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailContext _context;

        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, EmailContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        private async Task<ProfileViewModel> BuildModelAsync(AppUser user)
        {
            var email = user.Email;
            return new ProfileViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Description = user.Description,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserName = user.UserName,
                ImageUrl = user.ImageUrl,
                InboxCount = await _context.Messages.CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft),
                SentCount = await _context.Messages.CountAsync(x => x.SenderEmail == email && !x.DeletedBySender && !x.IsDraft),
                StarredCount = await _context.Messages.CountAsync(x => x.ReceiverEmail == email && !x.DeletedByReceiver && !x.IsDraft && x.IsFavourite)
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(await BuildModelAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                var fresh = await BuildModelAsync(user);
                model.Email = fresh.Email; model.UserName = fresh.UserName; model.ImageUrl = fresh.ImageUrl;
                model.InboxCount = fresh.InboxCount; model.SentCount = fresh.SentCount; model.StarredCount = fresh.StarredCount;
                return View(model);
            }

            user.Name = model.Name.Trim();
            user.Surname = model.Surname.Trim();
            user.Description = model.Description?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
                var fresh = await BuildModelAsync(user);
                model.Email = fresh.Email; model.UserName = fresh.UserName; model.ImageUrl = fresh.ImageUrl;
                model.InboxCount = fresh.InboxCount; model.SentCount = fresh.SentCount; model.StarredCount = fresh.StarredCount;
                return View(model);
            }

            TempData["ProfileOk"] = "Profil bilgilerin güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                TempData["PasswordError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["ProfileOk"] = "Şifren değiştirildi.";
            return RedirectToAction(nameof(Index));
        }

    }
}