using FlowKunevDev.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace FlowKunevDev.Services.BackgroundServices
{
    public class PlannedTransactionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PlannedTransactionBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Проверява всеки час

        public PlannedTransactionBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PlannedTransactionBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PlannedTransactionBackgroundService стартира.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPlannedTransactionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при обработката на планираните транзакции");
                }

                // Изчакваме до следващата проверка
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessPlannedTransactionsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var plannedTransactionService = scope.ServiceProvider.GetRequiredService<IPlannedTransactionService>();

            _logger.LogInformation("Започва обработка на планирани транзакции...");

            try
            {
                // Изпълняваме дължимите транзакции
                var executedCount = await plannedTransactionService.ExecuteDueTransactionsAsync();
                if (executedCount > 0)
                {
                    _logger.LogInformation($"Изпълнени са {executedCount} дължими транзакции.");
                }

                // Обработваме повтарящите се транзакции
                var recurringCount = await plannedTransactionService.ProcessRecurringTransactionsAsync();
                if (recurringCount > 0)
                {
                    _logger.LogInformation($"Създадени са {recurringCount} нови повтарящи се транзакции.");
                }

                if (executedCount == 0 && recurringCount == 0)
                {
                    _logger.LogDebug("Няма планирани транзакции за обработка.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при обработката на планираните транзакции");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PlannedTransactionBackgroundService спира.");
            await base.StopAsync(stoppingToken);
        }
    }
}