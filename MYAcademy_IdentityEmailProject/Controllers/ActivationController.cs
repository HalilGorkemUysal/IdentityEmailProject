using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class ActivationController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public ActivationController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult UserActivation()
        {
            var userEmail = TempData["Email"] as string;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("SignUp", "Register");
            }

            var model = new UserActivationViewModel
            {
                Email = userEmail,
            };
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> UserActivation(UserActivationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user == null || user.ValidCode != model.Code)
            {
                // Hata Code alanına bağlı → input'un altında görünür
                ModelState.AddModelError(nameof(model.Code), "Doğrulama kodu hatalı.");
                return View(model);
            }

            // Login'deki RequireConfirmedEmail kontrolü bu kolona bakıyor
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Hesabınız doğrulandı. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("SignIn", "Login");
        }
    }
}
