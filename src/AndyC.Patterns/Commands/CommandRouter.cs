using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AndyC.Patterns.Commands
{
    public class CommandRouter : ICommandRouter
    {
        private readonly ICommandHandlerFactory handlerFactory;

        public CommandRouter(ICommandHandlerFactory handlerFactory)
        {
            this.handlerFactory = handlerFactory;
        }

        public void ExecuteAction(IAction actionInstance)
        {
            var actionType = actionInstance.GetType();

            ExecuteAction(actionType, actionInstance);
        }

        public void ExecuteAction(Type actionType, object actionInstance)
        {
            ExecuteCommand(
                typeof(IActionHandler<>),
                ExecuteActionMethodTemplate,
                actionInstance,
                actionType
            );
        }

        public TOutput ExecuteFunction<TOutput>(IFunction<TOutput> functionInstance)
        {
            var functionType = functionInstance.GetType();

            return ExecuteFunction<TOutput>(functionType, functionInstance);
        }

        public TOutput ExecuteFunction<TOutput>(Type functionType, object functionInstance)
        {
            return
                (TOutput)ExecuteCommand(
                    typeof(IFunctionHandler<,>),
                    ExecuteFunctionMethodTemplate,
                    functionInstance,
                    functionType,
                    typeof(TOutput)
                );
        }

        public async Task ExecuteActionAsync(IAction actionInstance)
        {
            var actionType = actionInstance.GetType();

            await ExecuteActionAsync(actionType, actionInstance).ConfigureAwait(false);
        }

        public async Task ExecuteActionAsync(Type actionType, object actionInstance)
        {
            await ExecuteActionCommandAsync(
                typeof(IActionHandlerAsync<>),
                ExecuteActionMethodTemplateAsync,
                actionInstance,
                actionType
            );
        }

        public async Task<TOutput> ExecuteFunctionAsync<TOutput>(IFunction<TOutput> functionInstance)
        {
            var functionType = functionInstance.GetType();

            return await ExecuteFunctionAsync<TOutput>(functionType, functionInstance).ConfigureAwait(false);
        }

        public async Task<TOutput> ExecuteFunctionAsync<TOutput>(Type functionType, object functionInstance)
        {
            return await ExecuteFunctionCommandAsync<TOutput>(
                typeof(IFunctionHandlerAsync<,>),
                ExecuteFunctionMethodTemplateAsync,
                functionInstance,
                functionType,
                typeof(TOutput)
            );
        }

        /// <summary>
        /// Helper method to execute any ICommandHandler instance in a generic
        /// way.
        /// </summary>
        /// <param name="handlerTypeTemplate">
        /// Generic Handler Type template that we fill in (with the handler's
        /// TCommand and TResult) and use to resolve an instance of our desired
        /// <see cref="ICommandHandler" /> using the
        /// <see cref="ICommandHandlerFactory" />.
        /// </param>
        /// <param name="executeMethodTemplate">
        /// The pre-reflected template method used to call the actual generic
        /// Execute method of the <see cref="ICommandHandler" /> instance of
        /// unknown exact type.
        /// </param>
        /// <param name="commandInstance">
        /// The <see cref="ICommand" /> to be
        /// handled by the <see cref="ICommandHandler" />.</param>
        /// <param name="genericTemplateArgs">
        /// An array of Types to be used as generic type parameters when
        /// when filling in the <see cref="handlerTypeTemplate" /> and
        /// <see cref="executeMethodTemplate" />.
        /// </param>
        /// <returns>
        /// The results of executing the <see cref="ICommandHandler" />.
        /// </returns>
        private object ExecuteCommand(
            Type handlerTypeTemplate,
            MethodInfo executeMethodTemplate,
            object commandInstance,
            params Type[] genericTemplateArgs
        )
        {
            // Fill in handlerTypeTemplate with genericTemplateArgs to get the
            // generic handlerType we want to resolve
            var handlerType =
                handlerTypeTemplate.MakeGenericType(genericTemplateArgs);

            // Resolve the handlerInstance
            var handlerInstance = handlerFactory.Create(handlerType);

            // Fill in the handler's executeMethodTemplate with the same
            // genericTemplateArgs to get a method instance we can execute
            // against the resolved handlerInstance
            var executeMethod =
                executeMethodTemplate.MakeGenericMethod(genericTemplateArgs);

            try
            {
                return
                    executeMethod.Invoke(
                        null,
                        new[] { handlerInstance, commandInstance }
                    );
            }
            catch (TargetInvocationException ex)
            {
                throw new CommandExecutionException(commandInstance, ex.InnerException);
            }
            finally
            {
                handlerFactory.Release(handlerInstance);
            }
        }

        /// <summary>
        /// Helper method to execute any ICommandHandler instance in a generic
        /// way.
        /// </summary>
        /// <param name="handlerTypeTemplate">
        /// Generic Handler Type template that we fill in (with the handler's
        /// TCommand and TResult) and use to resolve an instance of our desired
        /// <see cref="ICommandHandler" /> using the
        /// <see cref="ICommandHandlerFactory" />.
        /// </param>
        /// <param name="executeMethodTemplate">
        /// The pre-reflected template method used to call the actual generic
        /// Execute method of the <see cref="ICommandHandler" /> instance of
        /// unknown exact type.
        /// </param>
        /// <param name="commandInstance">
        /// The <see cref="ICommand" /> to be
        /// handled by the <see cref="ICommandHandler" />.</param>
        /// <param name="genericTemplateArgs">
        /// An array of Types to be used as generic type parameters when
        /// when filling in the <see cref="handlerTypeTemplate" /> and
        /// <see cref="executeMethodTemplate" />.
        /// </param>
        /// <returns>
        /// The results of executing the <see cref="ICommandHandler" />.
        /// </returns>
        private async Task<TOutput> ExecuteFunctionCommandAsync<TOutput>(
            Type handlerTypeTemplate,
            MethodInfo executeMethodTemplate,
            object commandInstance,
            params Type[] genericTemplateArgs
        )
        {
            // Fill in handlerTypeTemplate with genericTemplateArgs to get the
            // generic handlerType we want to resolve
            var handlerType =
                handlerTypeTemplate.MakeGenericType(genericTemplateArgs);

            // Resolve the handlerInstance
            var handlerInstance = handlerFactory.Create(handlerType);

            // Fill in the handler's executeMethodTemplate with the same
            // genericTemplateArgs to get a method instance we can execute
            // against the resolved handlerInstance
            var executeMethod =
                executeMethodTemplate.MakeGenericMethod(genericTemplateArgs);

            try
            {
                return
                    await (Task<TOutput>)executeMethod.Invoke(
                        null,
                        new[] { handlerInstance, commandInstance }
                    );
            }
            catch (TargetInvocationException ex)
            {
                throw new CommandExecutionException(commandInstance, ex.InnerException);
            }
            finally
            {
                handlerFactory.Release(handlerInstance);
            }
        }

        /// <summary>
        /// Helper method to execute any ICommandHandler instance in a generic
        /// way.
        /// </summary>
        /// <param name="handlerTypeTemplate">
        /// Generic Handler Type template that we fill in (with the handler's
        /// TCommand and TResult) and use to resolve an instance of our desired
        /// <see cref="ICommandHandler" /> using the
        /// <see cref="ICommandHandlerFactory" />.
        /// </param>
        /// <param name="executeMethodTemplate">
        /// The pre-reflected template method used to call the actual generic
        /// Execute method of the <see cref="ICommandHandler" /> instance of
        /// unknown exact type.
        /// </param>
        /// <param name="commandInstance">
        /// The <see cref="ICommand" /> to be
        /// handled by the <see cref="ICommandHandler" />.</param>
        /// <param name="genericTemplateArgs">
        /// An array of Types to be used as generic type parameters when
        /// when filling in the <see cref="handlerTypeTemplate" /> and
        /// <see cref="executeMethodTemplate" />.
        /// </param>
        /// <returns>
        /// The results of executing the <see cref="ICommandHandler" />.
        /// </returns>
        private async Task ExecuteActionCommandAsync(
            Type handlerTypeTemplate,
            MethodInfo executeMethodTemplate,
            object commandInstance,
            params Type[] genericTemplateArgs
        )
        {
            // Fill in handlerTypeTemplate with genericTemplateArgs to get the
            // generic handlerType we want to resolve
            var handlerType =
                handlerTypeTemplate.MakeGenericType(genericTemplateArgs);

            // Resolve the handlerInstance
            var handlerInstance = handlerFactory.Create(handlerType);

            // Fill in the handler's executeMethodTemplate with the same
            // genericTemplateArgs to get a method instance we can execute
            // against the resolved handlerInstance
            var executeMethod =
                executeMethodTemplate.MakeGenericMethod(genericTemplateArgs);

            try
            {
                   await (Task)executeMethod.Invoke(
                        null,
                        new[] { handlerInstance, commandInstance }
                    );
            }
            catch (TargetInvocationException ex)
            {
                throw new CommandExecutionException(commandInstance, ex.InnerException);
            }
            finally
            {
                handlerFactory.Release(handlerInstance);
            }
        }


        private static readonly MethodInfo ExecuteFunctionMethodTemplate =
            ReflectionHelper.GetGenericMethodDefinition(() =>
                ExecuteFunctionShim<IFunction<object>, object>(null, null)
            );

        private static readonly MethodInfo ExecuteActionMethodTemplate =
            ReflectionHelper.GetGenericMethodDefinition(() =>
                ExecuteActionShim<IAction>(null, null)
            );

        private static TOutput ExecuteFunctionShim<TFunction, TOutput>(
            IFunctionHandler<TFunction, TOutput> handler,
            TFunction function
        ) 
            where TFunction : IFunction<TOutput>
        {
            return handler.Execute(function);
        }

        private static void ExecuteActionShim<TAction>(
            IActionHandler<TAction> handler,
            TAction action
        ) 
            where TAction : IAction
        {
            handler.Execute(action);
        }

        private static readonly MethodInfo ExecuteFunctionMethodTemplateAsync =
            ReflectionHelper.GetGenericMethodDefinition(() =>
                ExecuteFunctionShimAsync<IFunction<object>, object>(null, null)
            );

        private static readonly MethodInfo ExecuteActionMethodTemplateAsync =
            ReflectionHelper.GetGenericMethodDefinition(() =>
                ExecuteActionShimAsync<IAction>(null, null)
            );

        private static Task<TOutput> ExecuteFunctionShimAsync<TFunction, TOutput>(
            IFunctionHandlerAsync<TFunction, TOutput> handler,
            TFunction function
        ) 
            where TFunction : IFunction<TOutput>
        {
            return handler.ExecuteAsync(function);
        }

        private static Task ExecuteActionShimAsync<TAction>(
            IActionHandlerAsync<TAction> handler,
            TAction action
        ) 
            where TAction : IAction
        {
            return handler.ExecuteAsync(action);
        }

    }
}
