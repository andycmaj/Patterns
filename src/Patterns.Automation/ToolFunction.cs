using Cake.Core.Tooling;
using Patterns.Commands;

namespace Automation
{
    public abstract class ToolFunction<TSettings, TOutput> :
        IFunction<TOutput>
        where TSettings : ToolSettings, new()
    {
        public TSettings Settings { get; set; } = new TSettings();
    }
}
