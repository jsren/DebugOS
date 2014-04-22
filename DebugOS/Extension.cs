
namespace DebugOS
{
    public class Extension : IDebugExtension
    {
        private IDebugExtension extension;
        
        public bool LoadedSuccessfully { get; private set; }
        public string Name { get { return extension.Name; } }

        public Extension(IDebugExtension extension) {
            this.extension = extension;
        }

        public void Initialise(string[] args)
        {
            try 
            { 
                this.extension.Initialise(args);
                this.LoadedSuccessfully = true;
            }
            catch 
            {
                this.LoadedSuccessfully = false; 
                throw; 
            }
        }

        public void SetupUI(IDebugUI UI, IDebugger debugger) {
            this.extension.SetupUI(UI, debugger);
        }
    }
}
