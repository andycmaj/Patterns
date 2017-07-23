using System.Threading.Tasks;

namespace AndyC.Patterns.Commands
{

    /// <summary>
    /// Handles execution of IFunctions of type TFunction which return
    /// results of type TOutput.
    /// </summary>
    /// <typeparam name="TFunction">
    /// The type of IFunction this IFunctionHandler can execute.
    /// </typeparam>
    /// <typeparam name="TOutput">
    /// The type of result returned by the IFunction of type TFunction.
    /// </typeparam>
    public interface IFunctionHandler<in TFunction, out TOutput> : ICommandHandler
        where TFunction : IFunction<TOutput>
    {

        /// <summary>
        /// Execute the specified IFunction instance and return a result
        /// of type TOutput.
        /// </summary>
        TOutput Execute(TFunction function);
    }
}
