using Microsoft.EntityFrameworkCore;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;

namespace FlowKunevDev.Services.Implementations
{
    public class AccountTransferService : IAccountTransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountService _accountService;

        public AccountTransferService(ApplicationDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        public async Task<AccountTransferDto?> GetByIdAsync(int id, string userId)
        {
            var transfer = await _context.AccountTransfers
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            return transfer == null ? null : MapToDto(transfer);
        }

        public async Task<IEnumerable<AccountTransferDto>> GetAllAsync(string userId)
        {
            var transfers = await _context.AccountTransfers
                .Where(t => t.UserId == userId)
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return transfers.Select(MapToDto);
        }

        public async Task<IEnumerable<AccountTransferDto>> GetByAccountAsync(int accountId, string userId)
        {
            var transfers = await _context.AccountTransfers
                .Where(t => t.UserId == userId && (t.FromAccountId == accountId || t.ToAccountId == accountId))
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return transfers.Select(MapToDto);
        }

        public async Task<AccountTransferDto> CreateAsync(CreateAccountTransferDto createDto, string userId)
        {
            if (createDto.FromAccountId == createDto.ToAccountId)
            {
                throw new InvalidOperationException("Сметките трябва да бъдат различни.");
            }

            var fromAccountExists = await _accountService.ExistsAsync(createDto.FromAccountId, userId);
            var toAccountExists = await _accountService.ExistsAsync(createDto.ToAccountId, userId);

            if (!fromAccountExists || !toAccountExists)
            {
                throw new InvalidOperationException("Невалидни сметки за трансфер.");
            }

            if (!await _accountService.HasSufficientBalanceAsync(createDto.FromAccountId, userId, createDto.Amount))
            {
                throw new InvalidOperationException("Недостатъчен баланс по изходящата сметка.");
            }

            var transfer = new AccountTransfer
            {
                FromAccountId = createDto.FromAccountId,
                ToAccountId = createDto.ToAccountId,
                Amount = createDto.Amount,
                Date = createDto.Date,
                Description = createDto.Description,
                Notes = createDto.Notes,
                UserId = userId,
                CreatedDate = DateTime.Now
            };

            _context.AccountTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            await _context.Entry(transfer).Reference(t => t.FromAccount).LoadAsync();
            await _context.Entry(transfer).Reference(t => t.ToAccount).LoadAsync();

            return MapToDto(transfer);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var transfer = await _context.AccountTransfers
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transfer == null)
                return false;

            _context.AccountTransfers.Remove(transfer);
            await _context.SaveChangesAsync();
            return true;
        }

        private static AccountTransferDto MapToDto(AccountTransfer transfer)
        {
            return new AccountTransferDto
            {
                Id = transfer.Id,
                Amount = transfer.Amount,
                Date = transfer.Date,
                Description = transfer.Description,
                Notes = transfer.Notes,
                FromAccountId = transfer.FromAccountId,
                FromAccountName = transfer.FromAccount?.Name ?? string.Empty,
                ToAccountId = transfer.ToAccountId,
                ToAccountName = transfer.ToAccount?.Name ?? string.Empty
            };
        }
    }
}
