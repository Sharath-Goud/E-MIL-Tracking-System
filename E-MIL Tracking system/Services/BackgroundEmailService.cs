namespace E_MIL_Tracking_system.Services
{
    public class BackgroundEmailService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly ILogger<BackgroundEmailService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundEmailService(
            IBackgroundEmailQueue emailQueue,
            ILogger<BackgroundEmailService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _emailQueue = emailQueue;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var emailTask = await _emailQueue.DequeueAsync(stoppingToken);

                try
                {
                    await emailTask(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background email sending failed.");
                }
            }
        }
    }
}