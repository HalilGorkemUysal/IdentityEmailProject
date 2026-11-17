using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;

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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserActivation(int Code)
        {
            var userEmail = TempData["Email"].ToString();
            var user = await _userManager.FindByEmailAsync(userEmail);

            if(user.ValidCode == Code)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("SignIn", "Login");
            }

            TempData.Keep("Email");
            return View();
        }
    }
}
