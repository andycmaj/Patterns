using System;
using Patterns.Commands;

namespace Automation.Tests
{
    public class ActionFixture<TAction, TActionHandler> : CommandFixture
        where TAction : IAction
        where TActionHandler : IActionHandler<TAction>
    {
        public ActionFixture(string toolExecutable) : base(toolExecutable)
        {
        }

        public void RunCommand(TAction command)
        {
            var handler =
                (TActionHandler)Activator.CreateInstance(typeof(TActionHandler), Context);

            handler.Execute(command);
        }
    }
}
