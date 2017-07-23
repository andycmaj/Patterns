namespace AndyC.Patterns.Commands
{
    /// <summary>
    /// Represents a command that executes and returns a result of
    /// type TOutput.
    /// IAction is analogous to .NET's Action.
    /// </summary>
    /// <typeparam name="TOutput">
    /// The type of result the command should return. This allows generic
    /// execution of commands using the ICommandRouter without having to
    /// explicitly specify input and output type arguments.
    /// </typeparam>
    public interface IFunction<out TOutput> : ICommand
    {
    }
}
