using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Mapping
{
    public static class Mappings
    {
        public static AccountDto ToDto(this Account a, decimal currentBalance, int transactionCount, DateTime? lastTransactionDate) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description ?? string.Empty,
            InitialBalance = a.InitialBalance,
            CurrentBalance = currentBalance,
            Type = a.Type,
            Color = a.Color,
            UserId = a.UserId,
            CreatedDate = a.CreatedDate,
            IsActive = a.IsActive,
            TransactionCount = transactionCount,
            LastTransactionDate = lastTransactionDate
        };

        public static Account ToEntity(this CreateAccountDto dto) => new()
        {
            Name = dto.Name,
            Description = dto.Description,
            InitialBalance = dto.InitialBalance,
            Type = dto.Type,
            Color = dto.Color,
            CreatedDate = TimeHelper.UtcNow,
            IsActive = true
        };

        public static void ApplyTo(this UpdateAccountDto dto, Account target)
        {
            target.Name = dto.Name;
            target.Description = dto.Description;
            target.Type = dto.Type;
            target.Color = dto.Color;
            target.IsActive = dto.IsActive;
        }

        public static BudgetDto ToDto(this Budget b) => new()
        {
            Id = b.Id,
            Name = b.Name,
            Amount = b.Amount,
            CategoryId = b.CategoryId,
            CategoryName = b.Category?.Name ?? string.Empty,
            UserId = b.UserId,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            IsActive = b.IsActive,
            CreatedDate = b.CreatedDate
        };

        public static Budget ToEntity(this CreateBudgetDto dto) => new()
        {
            Name = dto.Name,
            Amount = dto.Amount,
            CategoryId = dto.CategoryId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = true,
            CreatedDate = TimeHelper.UtcNow
        };

        public static void ApplyTo(this UpdateBudgetDto dto, Budget target)
        {
            target.Id = dto.Id;
            target.Name = dto.Name;
            target.Amount = dto.Amount;
            target.CategoryId = dto.CategoryId;
            target.StartDate = dto.StartDate;
            target.EndDate = dto.EndDate;
            target.IsActive = dto.IsActive;
        }

        public static PlannedTransactionDto ToDto(this PlannedTransaction pt) => new()
        {
            Id = pt.Id,
            Description = pt.Description,
            PlannedAmount = pt.PlannedAmount,
            PlannedDate = pt.PlannedDate,
            CategoryId = pt.CategoryId,
            CategoryName = pt.Category?.Name ?? string.Empty,
            CategoryColor = pt.Category?.Color ?? "#007bff",
            CategoryIcon = pt.Category?.Icon ?? "fa fa-folder",
            AccountId = pt.AccountId,
            AccountName = pt.Account?.Name ?? string.Empty,
            UserId = pt.UserId,
            Type = pt.Type,
            Notes = pt.Notes,
            Status = pt.Status,
            IsRecurring = pt.IsRecurring,
            RecurrenceType = pt.RecurrenceType,
            ExecutedTransactionId = pt.ExecutedTransactionId,
            CreatedDate = pt.CreatedDate
        };

        public static PlannedTransactionSummaryDto ToSummaryDto(this PlannedTransaction pt)
        {
            var today = TimeHelper.LocalNow.Date;
            return new PlannedTransactionSummaryDto
            {
                Id = pt.Id,
                Description = pt.Description,
                PlannedAmount = pt.PlannedAmount,
                PlannedDate = pt.PlannedDate,
                CategoryName = pt.Category?.Name ?? string.Empty,
                CategoryColor = pt.Category?.Color ?? "#007bff",
                AccountName = pt.Account?.Name ?? string.Empty,
                Type = pt.Type,
                Status = pt.Status,
                IsOverdue = pt.Status == PlannedTransactionStatus.Planned && pt.PlannedDate.Date < today,
                IsDueToday = pt.Status == PlannedTransactionStatus.Planned && pt.PlannedDate.Date == today,
                DaysUntilDue = pt.Status == PlannedTransactionStatus.Planned ? (pt.PlannedDate.Date - today).Days : 0
            };
        }

        public static PlannedTransaction ToEntity(this CreatePlannedTransactionDto dto, string userId) => new()
        {
            Description = dto.Description,
            PlannedAmount = dto.PlannedAmount,
            PlannedDate = dto.PlannedDate,
            CategoryId = dto.CategoryId,
            AccountId = dto.AccountId,
            UserId = userId,
            Type = dto.Type,
            Notes = dto.Notes,
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.RecurrenceType,
            Status = PlannedTransactionStatus.Planned,
            CreatedDate = TimeHelper.UtcNow
        };

        public static void ApplyTo(this UpdatePlannedTransactionDto dto, PlannedTransaction target)
        {
            target.Id = dto.Id;
            target.Description = dto.Description;
            target.PlannedAmount = dto.PlannedAmount;
            target.PlannedDate = dto.PlannedDate;
            target.CategoryId = dto.CategoryId;
            target.AccountId = dto.AccountId;
            target.Type = dto.Type;
            target.Notes = dto.Notes;
            target.IsRecurring = dto.IsRecurring;
            target.RecurrenceType = dto.RecurrenceType;
        }
    }
}
