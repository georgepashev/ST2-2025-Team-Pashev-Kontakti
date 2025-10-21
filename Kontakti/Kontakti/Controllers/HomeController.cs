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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var contact = Database.Instance.GetContactById(id);
            if (contact == null)
            {
                return NotFound();
            }
            else
            {
                return View(contact);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Contact contact)
        {
            if (!ModelState.IsValid) {
                return View(contact);
            }
            else
            {
                try
                {
                    var ok = Database.Instance.UpdateContact(contact);
                    if (!ok) {
                        ModelState.AddModelError(string.Empty,

                            "Неуспешна редакция");
                        return View(contact);
                        
                    
                    }
                    return RedirectToAction("Index", "Home");

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Update failed: {ex.Message}"
                        );
                    return View(contact);
                }
            }
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var contact = Database.Instance.GetContactById(id);
            if (contact == null)
            {
                return NotFound();
            }
            else
            {
                return View(contact);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                try
                {
                    var ok = Database.Instance.DeleteContact(id);
                    if (!ok)
                    {
                        ModelState.AddModelError(string.Empty,

                            "Неуспешно изтриване");
                        return View();


                    }
                    return RedirectToAction("Index", "Home");

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Update failed: {ex.Message}"
                        );
                    return View();
                }
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
