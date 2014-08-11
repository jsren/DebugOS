/* IDebugUI.cs - (c) James S Renwick 2013
 * --------------------------------------
 * Version 1.5.0
 */
using System;
using System.Drawing;

namespace DebugOS
{
    /// <summary>
    /// Provides methods for augmenting and interactivity 
    /// with the graphical user interface.
    /// </summary>
    public interface IDebugUI
    {
        /// <summary>
        /// Gets the technology providing the GUI.
        /// </summary>
        GUIType Type { get; }
        /// <summary>
        /// Gets the name of the current theme, or null if themes
        /// not supported.
        /// </summary>
        string Theme { get; set; }

        /// <summary>
        /// Executes the given delegate on the UI thread, permitting
        /// access to thread-specific members.
        /// </summary>
        /// <param name="delegate">The delegate to execute.</param>
        /// <param name="args">The parameters to pass to the delegate.</param>
        void InvokeMethod(Delegate @delegate, params object[] args);
        
        /// <summary>
        /// Displays the given <see cref="IToolbarPanel"/> on the 
        /// user interface.
        /// </summary>
        /// <param name="panel">The panel to display.</param>
        void AddToolbarPanel(IToolbarPanel panel);
        /// <summary>
        /// Removes the given <see cref="IToolbarPanel"/> from the
        /// user interface.
        /// </summary>
        /// <param name="panel">The panel to remove.</param>
        void RemoveToolbarPanel(IToolbarPanel panel);
        /// <summary>
        /// Adds the given menu item to the user interface menu bar.
        /// </summary>
        /// <param name="item">The menu item to add.</param>
        /// <param name="path">
        /// The (\)-separated path under which the menu item will be placed.
        /// This should not include the label of the new menu item.
        /// All items on the path must already be present.
        /// </param>
        void AddMenuItem(IMenuItem item, string path);
        /// <summary>
        /// Adds the given UI panel to the user interface. A location
        /// preference must be given, but this is not garanteed to be met.
        /// </summary>
        /// <param name="panel">The panel to display.</param>
        /// <param name="preferredLocation">The location preference for the panel.</param>
        /// <returns>An integer handle for the panel.</returns>
        int AddMainPanel(IMainPanel panel, PanelLocation preferredLocation);
        /// <summary>
        /// Adds the given search category to the smart search box.
        /// </summary>
        /// <param name="category">The category to add.</param>
        void AddSearchCategory(ISearchCategory category);
        /// <summary>
        /// Brings the given panel into view and gives it focus.
        /// </summary>
        /// <param name="panelID">The handle of the panel upon which to focus.</param>
        void FocusMainPanel(int panelID);
        /// <summary>
        /// Removes the given panel from the user interface.
        /// </summary>
        /// <param name="panelID">The handle of the panel to remove.</param>
        void RemoveMainPanel(int panelID);
        /// <summary>
        /// Removes the given search category from the user interface.
        /// </summary>
        /// <param name="category">The <see cref="ISearchCategory"/> to remove.</param>
        void RemoveSearchCategory(ISearchCategory category);

        /// <summary>
        /// Tries to open a source code view loaded from the path given.
        /// If one is already open, brings it into view.
        /// </summary>
        /// <param name="sourcePath">The path to the source file to display.</param>
        void OpenSourceView(string sourcePath);
        /// <summary>
        /// Tries to open an assembly code view loaded from the code unit
        /// given. If one is already open, brings it into view.
        /// </summary>
        /// <param name="unit">The code unit to display.</param>
        void OpenAssemblyView(CodeUnit unit);

        /// <summary>
        /// Creates a new user interface panel which allows for the display
        /// of custom UI controls. 
        /// 
        /// Call <see cref="AddMainPanel"/>(...) to display the panel in the UI.
        /// </summary>
        /// <returns>A new instance of an <see cref="IMainPanel"/>.</returns>
        IMainPanel NewMainPanel();
        /// <summary>
        /// Creates a new toolbar panel upon which <see cref="IToolbarItem"/>s can
        /// be displayed.
        /// 
        /// Call <see cref="AddToolbarPanel"/>(...) to display the panel on the UI.
        /// </summary>
        /// <returns>A new instance of an <see cref="IToolbarPanel"/>.</returns>
        IToolbarPanel NewToolbarPanel();
        /// <summary>
        /// Creates a new menu item to be added to the menu bar.
        /// 
        /// Call <see cref="AddMenuItem"/>(...) to display the item on the UI.
        /// </summary>
        /// <param name="isToggle">When true, selection will toggle the menu item.</param>
        /// <returns>A new instance of an <see cref="IMenuItem"/>.</returns>
        IMenuItem NewMenuItem(bool isToggle = false);
        /// <summary>
        /// Creates a new toolbar item to be added to an <see cref="IToolbarPanel"/>.
        /// 
        /// Call <see cref="AddToolbarItem"/>(...) to display the item on the UI.
        /// </summary>
        /// <param name="isToggle">When true, selection will toggle the toolbar item.</param>
        /// <returns>A new instance of an <see cref="IToolbarItem"/>.</returns>
        IToolbarItem NewToolbarItem(bool isToggle = false);
    }

    public static class UIExtensionMethods
    {
        public static IMainPanel NewMainPanel(this IDebugUI ui, string title = null, 
            object content = null)
        {
            IMainPanel output = ui.NewMainPanel();
            output.Title   = title;
            output.Content = content;
            return output;
        }
        public static IMenuItem NewMenuItem(this IDebugUI ui, bool isToggle = false,
            string label = null, Bitmap icon = null, string shortcut = null, bool isEnabled = true)
        {
            IMenuItem output = ui.NewMenuItem(isToggle);
            output.Label     = label;
            output.Icon      = icon;
            output.Shortcut  = shortcut;
            output.IsEnabled = isEnabled;
            return output;
        }
    }
}
