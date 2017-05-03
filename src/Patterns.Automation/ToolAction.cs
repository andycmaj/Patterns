using Cake.Core.Tooling;
using Patterns.Commands;

namespace Automation
{
    public abstract class ToolAction<TSettings> : IAction
        where TSettings : ToolSettings, new()
    {
        public TSettings Settings { get; set; } = new TSettings();
    }
}
