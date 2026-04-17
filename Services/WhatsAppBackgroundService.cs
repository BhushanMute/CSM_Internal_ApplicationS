// Services/WhatsAppBackgroundService.cs
using CSMTutorial.Data.Repositories;
using CSMTutorial.Services;

namespace CSMTutorial.Services
{
    public class WhatsAppBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WhatsAppBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2); // Check every 2 minutes

        public WhatsAppBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<WhatsAppBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WhatsApp Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var whatsAppService = scope.ServiceProvider.GetRequiredService<IWhatsAppService>();

                    await whatsAppService.ProcessPendingNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WhatsApp background processing");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}