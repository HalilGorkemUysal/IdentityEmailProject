using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;

        public LoginController(SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult SignIn(string? returnUrl)
        { 
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserLoginViewModel userLoginViewModel,string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(userLoginViewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(
                userLoginViewModel.Username, userLoginViewModel.Password, 
                userLoginViewModel.RememberMe, true);

            if (result.Succeeded)
            {             
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Inbox", "Message");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Çok fazla hatalı deneme yaptınız. Hesabınız 5 dakika kilitlendi.");
            }

            //else if (result.IsNotAllowed)
            //{
            //    // Şifre doğru ama EmailConfirmed = false (RequireConfirmedEmail = true olduğu için)
            //    ModelState.AddModelError("", "Giriş yapabilmek için önce email adresinizi doğrulamalısınız.");
            //}

            else
            {
                // Hangisinin yanlış olduğunu söyleme
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            }

            return View(userLoginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Login");
        }
    }
}
