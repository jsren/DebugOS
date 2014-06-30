
namespace DebugOS
{
    public interface IExtension
    {
        string Name { get; }
        void Initialise(string[] args);
    }

    public interface IUIExtension : IExtension
    {
        void SetupUI(IDebugUI UI);
    }

    public interface IDebuggerExtension : IExtension
    {
        IDebugger LoadDebugger();
    }
}
