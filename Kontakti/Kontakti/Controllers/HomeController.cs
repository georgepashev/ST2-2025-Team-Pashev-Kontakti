using Kontakti.Data;
using Kontakti.Models;
using Kontakti.Services;
using Kontakti.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using System.Diagnostics;
using Database = Kontakti.Data.Database;

namespace Kontakti.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LlmClient _llm;


        public HomeController(ILogger<HomeController> logger, LlmClient llm)
        {
            _logger = logger;
            _llm = llm;
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

        [HttpGet]
        public IActionResult IntelligentSearch()
        {
            return View(new IntelligentSearchVm());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IntelligentSearch(IntelligentSearchVm model)

        {
            if (string.IsNullOrWhiteSpace(model.NaturalQuery))
            {
                model.Error = "Въведи естествен текст за търсене.";
                return View(model);
            }
            try
            {
                var json = await _llm.GetJsonPlanAsync(model.NaturalQuery!,
                Prompts.IntelligentSearchSystem);
                model.JsonPlan = json;
                var plan = System.Text.Json.JsonSerializer.Deserialize<QueryPlan>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (plan == null || plan.Filters == null || plan.Filters.Count == 0)
                {
                    model.Error = "LLM не върна валиден план за търсене.";
                    return View(model);
                }
                var results = Database.Instance.SelectContactsAdvanced(plan);
                model.Results = results;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Intelligent search failed.");
                model.Error = "Възникна грешка при интелигентното търсене.";
                return View(model);
            }
        }


    }
}
