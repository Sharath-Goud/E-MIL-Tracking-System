using System.Threading.Channels;

namespace E_MIL_Tracking_system.Services
{
    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue =
            Channel.CreateUnbounded<Func<CancellationToken, Task>>();

        public void QueueEmail(Func<CancellationToken, Task> emailTask)
        {
            _queue.Writer.TryWrite(emailTask);
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}