using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Web.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [Display(Name = "Потребителско име")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Невалиден телефонен номер.")]
        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        public string? StatusMessage { get; set; }
    }
}
