
namespace DebugOS
{
    public struct UIHandle<T> where T : class
    {
        public int Handle { get; private set; }
        public T Instance { get; private set; }

        public UIHandle(int handle, T instance) : this()
        {
            this.Handle   = handle;
            this.Instance = instance;
        }
    }
}