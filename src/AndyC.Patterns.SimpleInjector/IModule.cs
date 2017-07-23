using SimpleInjector;

namespace AndyC.Patterns.SimpleInjector
{
    public interface IModule
    {
        void RegisterServices(Container container);
    }
}
