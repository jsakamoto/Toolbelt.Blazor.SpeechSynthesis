using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Toolbelt.Blazor.SpeechSynthesis.Internals
{
    internal static class ValueTaskExtension
    {
        public static void WithLogException<T>(this Task<T> task, ILogger logger) => WithLogException(task as Task, logger);

        public static void WithLogException(this Task task, ILogger logger)
        {
            task.ConfigureAwait(false).GetAwaiter().OnCompleted(() =>
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    var e = task.Exception;
                    logger.LogError(e, e.Message);
                }
            });
        }
    }
}
