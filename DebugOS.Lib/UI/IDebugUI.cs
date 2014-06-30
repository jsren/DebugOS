using System;
using System.Drawing;
using System.Windows.Forms;

namespace DebugOS
{
    public interface IDebugUI
    {
        GUIType Type { get; }
        string Theme { get; set; }

        void InvokeMethod(Delegate @delegate, params object[] args);
        
        void AddToolbarPanel(IToolbarPanel panel);
        void RemoveToolbarPanel(IToolbarPanel panel);

        void AddMenuItem(string path, IMenuItem item);
        int AddMainPanel(PanelLocation preferredLocation, IMainPanel panel);

        void RemoveMainPanel(int panelID);
        void FocusMainPanel(int panelID);

        IMainPanel NewMainPanel();
        IToolbarPanel NewToolbarPanel();
        IMenuItem NewMenuItem(bool isToggle = false);
        IToolbarItem NewToolbarItem(bool isToggle = false);
    }
}
