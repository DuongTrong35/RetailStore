using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailStore.Models;

namespace RetailStore.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                return View();

            }
            int idrole = 1;
            if (user.Role == "staff") idrole = 2;
            HttpContext.Session.SetInt32("NumberRole", idrole);
            HttpContext.Session.SetString("UserName", user.Username);
            HttpContext.Session.SetString("UserRole", user.FullName);
            HttpContext.Session.SetString("NameRole", user.Role);


            return RedirectToAction("Index", "Home");
        }
    }
}
