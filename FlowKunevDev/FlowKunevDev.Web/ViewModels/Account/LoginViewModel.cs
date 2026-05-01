using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Web.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [Display(Name = "Потребителско име или имейл")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
