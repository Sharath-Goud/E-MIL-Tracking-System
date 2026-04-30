namespace E_MIL_Tracking_system.Services
{
    public class BackgroundEmailService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly ILogger<BackgroundEmailService> _logger;

        public BackgroundEmailService(
            IBackgroundEmailQueue emailQueue,
            ILogger<BackgroundEmailService> logger)
        {
            _emailQueue = emailQueue;
            _logger = logger;
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