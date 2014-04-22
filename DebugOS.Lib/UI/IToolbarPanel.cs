
namespace DebugOS
{
    public interface IToolbarPanel
    {
        string Title { get; set; }
        bool IsEnabled { get; set; }
        IToolbarItem[] Items { get; }

        void AddToolbarItem(IToolbarItem item);
        void RemoveToolbarItem(IToolbarItem item);
    }
}
