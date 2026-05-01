using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Web.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [DataType(DataType.Password)]
        [Display(Name = "Текуща парола")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето {0} е задължително.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Полето {0} трябва да е поне {2} символа.")]
        [DataType(DataType.Password)]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Потвърди новата парола")]
        [Compare(nameof(NewPassword), ErrorMessage = "Паролата и потвърждението не съвпадат.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? StatusMessage { get; set; }
    }
}
