using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;
using System.Net;
using System.Net.Mail;

namespace RetailStore.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= LOGIN =================

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            //var user = _context.Users.FirstOrDefault(u => u.Username == username);
            //if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            //{
            //    ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
            //    return View();
            //}
            var user = _context.Users
    .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                return View();
            }
            // 3️⃣ Giữ nguyên logic cũ của bạn
            int idrole = 1;
            if (user.Role == "staff") idrole = 2;
            else if (user.Role == "sales") idrole = 3;
            else if (user.Role == "importer") idrole = 4;

            string nametat = string.Concat(
                user.FullName
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => word[0])
            ).ToUpper();

            HttpContext.Session.SetInt32("NumberRole", idrole);
            HttpContext.Session.SetString("UserName", user.Username);
            HttpContext.Session.SetString("UserRole", user.FullName);
            HttpContext.Session.SetString("NameRole", user.Role);
            HttpContext.Session.SetInt32("nvid", user.UserId);
            HttpContext.Session.SetString("ShortName", nametat);

            return RedirectToAction("Index", "Home");
        }

        // ================= FORGOT PASSWORD =================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Username, string Email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == Username && u.Email == Email);

            if (user == null)
            {
                ViewBag.Error = "Username hoặc Email không đúng";
                return View();
            }

            // 1️⃣ Tạo token
            string token = Guid.NewGuid().ToString();

            var resetToken = new ResetPasswordToken
            {
                UserId = user.UserId,
                Token = token,
                ExpiredAt = DateTime.Now.AddMinutes(15),
                IsUsed = false
            };

            _context.ResetPasswordTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // 2️⃣ Tạo link reset
            string resetLink = Url.Action(
                "ResetPassword",
                "Login",
                new { token = token },
                Request.Scheme
            );

            // 3️⃣ Gửi email
            await SendEmailAsync(
                user.Email,
                "Đặt lại mật khẩu - Konoha Market",
                $@"
                <p>Xin chào <b>{user.Username}</b>,</p>
                <p>Bạn đã yêu cầu đặt lại mật khẩu.</p>
                <p>Click vào link bên dưới:</p>
                <p>
                    <a href='{resetLink}'>Đặt lại mật khẩu</a>
                </p>
                <p>Link có hiệu lực trong <b>15 phút</b>.</p>
                <p>Nếu không phải bạn yêu cầu, hãy bỏ qua email này.</p>
                "
            );

            ViewBag.Success = "Đã gửi link đặt lại mật khẩu qua email";
            return View();
        }

        // ================= RESET PASSWORD =================

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var reset = _context.ResetPasswordTokens
                .FirstOrDefault(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiredAt > DateTime.Now
                );

            if (reset == null)
                return Content("Link không hợp lệ hoặc đã hết hạn");

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(
            string token,
            string newPassword,
            string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu không khớp";
                ViewBag.Token = token;
                return View();
            }

            var reset = await _context.ResetPasswordTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiredAt > DateTime.Now
                );

            if (reset == null)
                return Content("Token không hợp lệ");

            //  Hash mật khẩu
            //reset.User.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            //reset.IsUsed = true;
            //await _context.SaveChangesAsync();

            reset.User.Password = newPassword;
            reset.IsUsed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ================= SEND EMAIL =================

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = "duongtrongtan352004@gmail.com";
            var appPassword = "zdshyxnvfeaarbki"; // Gmail App Password

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, "Konoha Market"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            await smtp.SendMailAsync(mail);
        }
    }
}
