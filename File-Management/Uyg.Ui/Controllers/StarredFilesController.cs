using Microsoft.AspNetCore.Mvc;
namespace UygUi.Controllers
{
    public class StarredFilesController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.ApiBaseURL = "https://localhost:7222/api"; // 7278 değil, 7222 olmalı
            return View();
        }
    }
}