using AutoMapper;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FlowKunevDev.Web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Account mappings
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore()) // Ще се изчислява отделно
                .ForMember(dest => dest.TransactionCount, opt => opt.Ignore())
                .ForMember(dest => dest.LastTransactionDate, opt => opt.Ignore());

            CreateMap<CreateAccountDto, Account>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateAccountDto, Account>()
                .ForMember(dest => dest.InitialBalance, opt => opt.Ignore()) // Не може да се променя
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<Account, AccountSummaryDto>()
                .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore());

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color))
                .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.Icon))
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account.Name));

            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore());

            CreateMap<UpdateTransactionDto, Transaction>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore());

            CreateMap<Transaction, TransactionSummaryDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color))
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account.Name));

            // Category mappings
            CreateMap<Category, CategorySummaryDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionCount, opt => opt.Ignore())
                .ForMember(dest => dest.Percentage, opt => opt.Ignore());
            // AccountTransfer mappings
            CreateMap<AccountTransfer, AccountTransferDto>()
                .ForMember(dest => dest.FromAccountName, opt => opt.MapFrom(src => src.FromAccount.Name))
                .ForMember(dest => dest.FromAccountColor, opt => opt.MapFrom(src => src.FromAccount.Color))
                .ForMember(dest => dest.ToAccountName, opt => opt.MapFrom(src => src.ToAccount.Name))
                .ForMember(dest => dest.ToAccountColor, opt => opt.MapFrom(src => src.ToAccount.Color));

            CreateMap<CreateAccountTransferDto, AccountTransfer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.FromAccount, opt => opt.Ignore())
                .ForMember(dest => dest.ToAccount, opt => opt.Ignore());

            CreateMap<UpdateAccountTransferDto, AccountTransfer>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.FromAccount, opt => opt.Ignore())
                .ForMember(dest => dest.ToAccount, opt => opt.Ignore());

            // Budget mappings
            CreateMap<Budget, BudgetDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SpentAmount, opt => opt.Ignore());

            CreateMap<CreateBudgetDto, Budget>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateBudgetDto, Budget>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
            // PlannedTransaction mappings (когато ги създадем)

        }
    }
}