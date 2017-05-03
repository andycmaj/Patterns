using Automation.Layer0;

namespace Automation.Extensions
{
    public static class WaitableCommandExtensions
    {
        public static IWaitableCommand WithLayer0Wait(
            this IWaitableCommand command,
            int waitTimeInMinutes
        )
        {
            command.WaitTimeInMinutes = waitTimeInMinutes;
            return command;
        }
    }
}
