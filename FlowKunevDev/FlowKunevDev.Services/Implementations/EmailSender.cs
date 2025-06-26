using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FlowKunevDev.Services.Implementations
{
    /// <summary>
    /// Simple email sender that logs emails to the configured logger.
    /// This ensures identity pages can reset passwords without configuring
    /// a real SMTP provider.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Sending email to {Email} with subject '{Subject}'.", email, subject);
            _logger.LogDebug("Email body: {Body}", htmlMessage);
            return Task.CompletedTask;
        }
    }
}
