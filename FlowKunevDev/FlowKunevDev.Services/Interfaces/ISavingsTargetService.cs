using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Interfaces
{
    public interface ISavingsTargetService
    {
        Task<SavingsTargetDto> GetCurrentMonthTargetAsync(string userId);
    }
}
