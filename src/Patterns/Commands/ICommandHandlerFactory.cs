using System;

namespace Patterns.Commands
{
    public interface ICommandHandlerFactory
    {
        ICommandHandler Create(Type handlerType);

        void Release(ICommandHandler handler);
    }
}
