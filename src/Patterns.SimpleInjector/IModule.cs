using SimpleInjector;

namespace Patterns.SimpleInjector
{
    public interface IModule
    {
        void RegisterServices(Container container);
    }
}
