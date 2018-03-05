using WindowsServer.Log;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Forget(this Task task, bool logException = false)
        {
            if (logException)
            {
                task.ContinueWith(
                    (t) => {
                        if (t.Exception != null)
                        {
                            _logger.ErrorException("Failed to run the task. An exception occurred.", t.Exception);
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
