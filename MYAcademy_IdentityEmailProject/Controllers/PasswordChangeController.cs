using MailKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MYAcademy_IdentityEmailProject.Entities;
using MYAcademy_IdentityEmailProject.Models.PasswordChangeModels;

namespace MYAcademy_IdentityEmailProject.Controllers
{
    public class PasswordChangeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public PasswordChangeController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }


        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Email);

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var passwordResetTokenLink = Url.Action("ResetPassword", "PasswordChange", new
            {//urlAction gidilecek bağlantıyı oluşturu. O sayfaya yönlendirmez!(Bağlantı hazırlığı yani.)
                userId = user.Id,
                token = passwordResetToken
            }, HttpContext.Request.Scheme);

            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxFromAddress = new MailboxAddress("Mailora Yönetim Paneli", "halilgorkemuysal@gmail.com");
            mimeMessage.From.Add(mailboxFromAddress);

            MailboxAddress mailboxToAddress = new MailboxAddress("User", forgetPasswordViewModel.Email);
            mimeMessage.To.Add(mailboxToAddress);


            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = $"Şifre Sıfırlama Bağlantısı: {passwordResetTokenLink}";
            mimeMessage.Body = bodyBuilder.ToMessageBody();
            mimeMessage.Subject = "Mailora Şifremi Sıfırla Talebi";

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 587, false);
            smtpClient.Authenticate("halilgorkemuysal@gmail.com", "pzpf tmmd fmwc vpel");
            smtpClient.Send(mimeMessage);
            smtpClient.Disconnect(true);

            return RedirectToAction("CheckYourEmail");
        }

        [HttpGet]
        public IActionResult CheckYourEmail()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            string userId = TempData.Peek("userId").ToString();
            string token = TempData.Peek("token").ToString();

            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordViewModel.Password);

            if (result.Succeeded)
            {
                TempData.Remove("userId");
                TempData.Remove("token");

                TempData["PasswordResetSuccess"] = true;

                return RedirectToAction("ResetPasswordSuccess");
            }
            
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ResetPasswordSuccess()
        {

            if (TempData["PasswordResetSuccess"] is not bool success || !success)
            {
                return RedirectToAction("SignIn", "Login");
            }
            return View();
        }
    }
}
