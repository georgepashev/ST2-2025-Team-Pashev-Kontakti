using Kontakti.Models;
namespace Kontakti.ViewModels;
public class IntelligentSearchVm
{

    public string? NaturalQuery { get; set; }
    public string? JsonPlan { get; set; } // дебъг/преглед на JSON от LLM
    public List<Contact>? Results { get; set; }
    public string? Error { get; set; }
}