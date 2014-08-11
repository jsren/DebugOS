using System;

namespace DebugOS
{

    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = true)]
    public sealed class ExtensionTypeAttribute : Attribute
    {
        public string Name { get; private set; }

        public ExtensionTypeAttribute(string typeName)
        {
            this.Name = typeName;
        }
    }


    public interface IExtension
    {
        string Name { get; }
        void Initialise(string[] args);
    }

    [ExtensionType("UI")]
    public interface IUIExtension : IExtension
    {
        void SetupUI(IDebugUI UI);
    }

    [ExtensionType("Debugger")]
    public interface IDebuggerExtension : IExtension
    {
        new string Name { get; }
        IDebugger LoadDebugger();
    }

    [ExtensionType("Resource Loader")]
    public interface ILoaderExtension : IExtension
    {
        bool CanLoad(string filepath);
        IDebugResource[] LoadResources(string filepath);

        T[] LoadResources<T>(string filepath) where T : IDebugResource;
    }
}
