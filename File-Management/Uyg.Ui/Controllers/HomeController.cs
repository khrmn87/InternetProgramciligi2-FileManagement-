using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UygUi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }


        [Route("Login")]
        public IActionResult Login()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Profile")]
        public IActionResult Profile()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }
    }
}
