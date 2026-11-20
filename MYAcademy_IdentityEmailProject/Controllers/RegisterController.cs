using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models;
using System.Security.Cryptography;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(UserManager<AppUser> userManager, ILogger<RegisterController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterViewModel userRegisterViewModel)
        {
            // 1) ViewModel attribute'larına uymuyorsa formu dolu şekilde geri göster
            if (!ModelState.IsValid)
            {
                return View(userRegisterViewModel);
            }

            int validCode = RandomNumberGenerator.GetInt32(100000, 1000000);

            AppUser appUser = new AppUser()
            {
                Name = userRegisterViewModel.Name,
                Surname = userRegisterViewModel.Surname,
                Email = userRegisterViewModel.Email,
                UserName = userRegisterViewModel.Username,
                ValidCode = validCode
            };

            var result = await _userManager.CreateAsync(appUser, userRegisterViewModel.Password);

            // 2) Program.cs kurallarına takıldıysa (şifre, aynı email, aynı kullanıcı adı) hataları göster
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);//hata tüm forma ait olur.
                }
                return View(userRegisterViewModel);
            }

            try
            {
                MimeMessage mimeMessage = new MimeMessage();
                MailboxAddress mailboxAddressFrom = new MailboxAddress("Mailora Yönetim Paneli", "halilgorkemuysal@gmail.com");
                mimeMessage.From.Add(mailboxAddressFrom);

                MailboxAddress mailboxAddressTo = new MailboxAddress("User", userRegisterViewModel.Email);
                mimeMessage.To.Add(mailboxAddressTo);

                mimeMessage.Subject = "Email Doğrulama Kodu";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = "Sisteme Giriş yapabilmek için gerekli olan doğrulama kodu: " + validCode;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Connect("smtp.gmail.com", 587, false);
                smtpClient.Authenticate("halilgorkemuysal@gmail.com", "kgox upex ieym yekp");
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(true);

            }
            catch (Exception ex)
            {
                // Mail gitmediyse kaydı geri al, kullanıcı tekrar deneyebilsin
                await _userManager.DeleteAsync(appUser);
                _logger.LogError(ex, "Doğrulama maili gönderilemedi: {Email}", userRegisterViewModel.Email);

                ModelState.AddModelError("", "Doğrulama maili gönderilemedi. Lütfen daha sonra tekrar deneyin.");
                return View(userRegisterViewModel);
            }
            //return RedirectToAction("SignIn","Login");

            TempData["Email"] = userRegisterViewModel.Email;
            return RedirectToAction("UserActivation", "Activation");
        }
    }
}