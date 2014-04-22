
namespace DebugOS
{
    public interface IDebugExtension
    {
        string Name { get; }

        void Initialise(string[] args);
        void SetupUI(IDebugUI UI, IDebugger debugger);
    }
}
