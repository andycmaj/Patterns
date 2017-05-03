using System;

namespace Patterns.Commands
{
    /// <summary>
    /// Indicates an unhandled Exception was thrown while a
    /// CommandHandler was executing a Command.
    /// </summary>
    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(
            object commandInstance,
            Exception innerException
        )
            : base(
                "Exception thrown while executing a command: " +
                    commandInstance.GetType().FullName,
                innerException
            )
        {
        }
    }
}
