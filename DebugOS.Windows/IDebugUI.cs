
namespace DebugOS.Windows
{
    public interface IDebugUI : DebugOS.IDebugUI
    {
        new IToolbarItem NewToolbarItem(bool isToggle);
    }
}
