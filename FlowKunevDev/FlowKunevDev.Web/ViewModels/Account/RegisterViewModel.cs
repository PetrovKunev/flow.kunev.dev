using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Web.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [StringLength(64, MinimumLength = 3, ErrorMessage = "Полето {0} трябва да е между {2} и {1} символа.")]
        [Display(Name = "Потребителско име")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Полето {0} трябва да е поне {2} символа.")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Потвърди паролата")]
        [Compare(nameof(Password), ErrorMessage = "Паролата и потвърждението не съвпадат.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
