using System.ComponentModel.DataAnnotations;

namespace Kontakti.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-zа-яА-Я \\-]{1,49}$", ErrorMessage = "Невалидно име")]
        [Display(Name ="Име")]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Електронна поща")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{5,15}$", ErrorMessage ="Невалиден телефонен номер")]
        [Display(Name = "Телефонен номер")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Адрес")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Адрес ред 2")]
        public string AddressLine2 { get; set; }

    }
}
