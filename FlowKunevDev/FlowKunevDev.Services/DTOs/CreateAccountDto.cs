using FlowKunevDev.Common;
using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Services.DTOs
{
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "Името на сметката е задължително")]
        [StringLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Описанието не може да бъде по-дълго от 500 символа")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Началният баланс е задължителен")]
        [Range(0, double.MaxValue, ErrorMessage = "Началният баланс не може да бъде отрицателен")]
        public decimal InitialBalance { get; set; }

        [Required(ErrorMessage = "Типът на сметката е задължителен")]
        public AccountType Type { get; set; }

        [Required(ErrorMessage = "Валутата е задължителна")]
        [RegularExpression("^(BGN|EUR|USD)$", ErrorMessage = "Валутата трябва да бъде BGN, EUR или USD")]
        public string Currency { get; set; } = "BGN";

        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Цветът трябва да бъде в hex формат")]
        public string Color { get; set; } = "#007bff";
    }
}