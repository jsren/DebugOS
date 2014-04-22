using System.Windows;

namespace DebugOS
{
    public interface IDebugUI
    {
        string Theme { get; set; }

        void AddToolbarPanel(IToolbarPanel panel);
        void RemoveToolbarPanel(IToolbarPanel panel);

        int AddMainPanel(PanelLocation preferredLocation, UIElement panel);
        void RemoveMainPanel(int panelID);

        IToolbarPanel NewToolbarPanel();
        IToolbarItem NewToolbarItem();
        IToolbarItem NewToolbarItem(bool isToggle);
    }
}
