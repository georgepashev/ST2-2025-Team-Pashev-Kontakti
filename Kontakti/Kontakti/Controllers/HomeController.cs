using System.Data.Entity;
using System.Diagnostics;
using Kontakti.Models;
using Microsoft.AspNetCore.Mvc;
using Kontakti.Data;
using Database = Kontakti.Data.Database;

namespace Kontakti.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var contacts = Database.Instance.GetAllContacts();

            return View(contacts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Contact());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return View(contact);
            }
            else
            {
                try
                {
                    var id = Database.Instance.AddContact(contact);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Insert Failed: {ex.Message}");
                }                

                return RedirectToAction("Index", "Home");

            }
        }

        public IActionResult Privacy()
        {
           return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
