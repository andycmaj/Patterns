using System;
using Patterns.Commands;

namespace Automation.Tests
{
    public class FunctionFixture<TFunction, TResult, TFunctionHandler> : CommandFixture
        where TFunction : IFunction<TResult>
        where TFunctionHandler : IFunctionHandler<TFunction, TResult>
    {
        public FunctionFixture(string toolExecutable) : base(toolExecutable)
        {
        }

        public TResult RunCommand(TFunction command)
        {
            var handler =
                (TFunctionHandler)Activator.CreateInstance(typeof(TFunctionHandler), Context);

            return handler.Execute(command);
        }
    }
}
