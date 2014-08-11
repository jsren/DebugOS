using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugOS.Windows.DIA
{
    public class ObjectFileLoader : ILoaderExtension, IUIExtension
    {
        public string Name
        {
            get { return "DIA Resource Loader"; }
        }

        public bool CanLoad(string filepath)
        {
            string ext = System.IO.Path.GetExtension(filepath);
            return ext == ".exe" || ext == ".pdb";
        }

        public IDebugResource[] LoadResources(string filepath)
        {
            throw new NotImplementedException();
        }

        public T[] LoadResources<T>(string filepath) where T : IDebugResource
        {
            throw new NotImplementedException();
        }

        public void Initialise(string[] args)
        {
            if (!DIALoader.DIASupported)
            {
                throw new NotSupportedException("DIA is not supported on this machine.");
            }
        }

        void IUIExtension.SetupUI(DebugOS.IDebugUI UI)
        {
            throw new NotImplementedException();
        }

        string IExtension.Name
        {
            get { throw new NotImplementedException(); }
        }

        void IExtension.Initialise(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
