using System.Threading.Tasks;

namespace Patterns.Commands
{
    /// <summary>
    /// Handles execution of IActions of type TAction.
    /// </summary>
    /// <typeparam name="TAction">
    /// The type of IAction this IActionHandler can execute.
    /// </typeparam>
    public interface IActionHandlerAsync<in TAction> : ICommandHandler
        where TAction : IAction
    {
        /// <summary>
        /// Execute the specified IAction instance.
        /// </summary>
        Task ExecuteAsync(TAction action);
    }
}
