using System;
using System.Threading.Tasks;

namespace Patterns.Commands
{

    public interface ICommandRouter
    {
        /// <summary>
        /// Execute an IAction command.
        /// </summary>
        void ExecuteAction(IAction actionInstance);

        /// <summary>
        /// Lookup and execute an IActionHandler implementation that matches the
        /// specified IActionHandler Type. Use the handler to execute the
        /// specified IAction instance.
        /// </summary>
        void ExecuteAction(Type handlerType, object actionInstance);

        /// <summary>
        /// Execute an IFunction command that returns a result of type
        /// TOutput.
        /// </summary>
        TOutput ExecuteFunction<TOutput>(IFunction<TOutput> functionInstance);

        /// <summary>
        /// Lookup and execute an IFunctionHandler implementation that matches the
        /// specified IFunctionHandler Type. Use the handler to execute the
        /// specified IFunction instance.
        /// </summary>
        TOutput ExecuteFunction<TOutput>(Type handlerType, object functionInstance);

        /// <summary>
        /// Execute an IAction command.
        /// </summary>
        Task ExecuteActionAsync(IAction actionInstance);

        /// <summary>
        /// Lookup and execute an IActionHandler implementation that matches the
        /// specified IActionHandler Type. Use the handler to execute the
        /// specified IAction instance.
        /// </summary>
        Task ExecuteActionAsync(Type handlerType, object actionInstance);

        /// <summary>
        /// Execute an IFunction command that returns a result of type
        /// TOutput.
        /// </summary>
        Task<TOutput> ExecuteFunctionAsync<TOutput>(IFunction<TOutput> functionInstance);

        /// <summary>
        /// Lookup and execute an IFunctionHandler implementation that matches the
        /// specified IFunctionHandler Type. Use the handler to execute the
        /// specified IFunction instance.
        /// </summary>
        Task<TOutput> ExecuteFunctionAsync<TOutput>(Type handlerType, object functionInstance);
    }
}

