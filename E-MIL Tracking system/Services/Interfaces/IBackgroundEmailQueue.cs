namespace E_MIL_Tracking_system.Services
{
    public interface IBackgroundEmailQueue
    {
        void QueueEmail(Func<CancellationToken, Task> emailTask);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}