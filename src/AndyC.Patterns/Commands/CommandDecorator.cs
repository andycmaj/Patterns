using System;

namespace AndyC.Patterns.Commands
{
    /// <summary>
    /// CommandRegistrator ignores handlers with this attribute
    /// This can be used when you want to define a decorator for a command for instance
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandDecorator : Attribute
    {
    }
}
