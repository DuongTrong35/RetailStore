using Microsoft.AspNetCore.Mvc;

namespace RetailStore.Controllers
{
    public class TrangChuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult HomeEmployee()
        {
            return PartialView();
        }
    }
}
