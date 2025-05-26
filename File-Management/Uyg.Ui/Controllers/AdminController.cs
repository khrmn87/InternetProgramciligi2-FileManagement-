using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UygUi.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Categories")]
        public IActionResult Categories()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }

        [Route("Files/{id}")]
        [Route("Files")]
        public IActionResult Files()
        {
            var ApiBaseURL = _configuration["ApiBaseURL"];
            ViewBag.ApiBaseURL = ApiBaseURL;
            return View();
        }
        public IActionResult StarredFiles()
        {
            ViewBag.ApiBaseURL = "https://localhost:7278/api";
            return View();
        }
    }


}
