using Cake.Core;
using Cake.Core.IO;

namespace Automation.Extensions
{
    public static class BuilderExtensions
    {
        public static ProcessArgumentBuilder AppendWaitTime(
            this ProcessArgumentBuilder builder,
            int? waitTimeInMinutes
        )
        {
            if (waitTimeInMinutes != null)
            {
                builder.AppendSwitch("--timeout", $"{waitTimeInMinutes}m");
            }
            return builder;
        }

        public static ProcessArgumentBuilder AppendWaitFlag(
            this ProcessArgumentBuilder builder,
            int? waitTimeInMinutes
        )
        {
            if (waitTimeInMinutes != null)
            {
                builder.Append("--wait");
            }
            return builder;
        }
    }
}