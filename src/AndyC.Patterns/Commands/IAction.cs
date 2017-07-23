namespace AndyC.Patterns.Commands
{
    /// <summary>
    /// Marker interface for an Action Command. Represents a command that
    /// executes an action and returns no results.
    ///
    /// IAction is analogous to .NET's <see cref="System.Action" />.
    /// </summary>
    public interface IAction : ICommand
    {
    }
}
