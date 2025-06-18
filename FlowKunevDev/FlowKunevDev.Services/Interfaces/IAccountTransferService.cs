using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Interfaces
{
    public interface IAccountTransferService
    {
        Task<AccountTransferDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<AccountTransferDto>> GetAllAsync(string userId);
        Task<IEnumerable<AccountTransferDto>> GetByAccountAsync(int accountId, string userId);
        Task<AccountTransferDto> CreateAsync(CreateAccountTransferDto createDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
