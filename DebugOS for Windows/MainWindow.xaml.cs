/* MainWindow.xaml.cs - (c) James S Renwick 2014
 * ---------------------------------------------
 * Version 1.5.1
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using WinState = System.Windows.WindowState;

namespace DebugOS
{
	public partial class MainWindow : Window, Windows.IDebugUI
	{
        private DragBehaviour drag;
        private string currentTheme;

        /// <summary>
        /// Window is hidden until loading complete.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            App.MainWindow = this;

			// Initialise titlebar buttons
			this.closeButton.Background.Opacity    = 0;
			this.maximiseButton.Background.Opacity = 0;
			this.minimiseButton.Background.Opacity = 0;
			
            // Allow the titlebar to drag the window
            this.drag = new DragBehaviour(this.titlebar, this);

            // Add initial top-level menu items
            menubar.AddItem("", new MenuItem() { Header = "_Debug" });
            menubar.AddItem("", new MenuItem() { Header = "_Settings" });
            menubar.AddItem("", new MenuItem() { Header = "_Help" });

            // Set the current theme - the built-in will be the first
            this.currentTheme = App.LoadedThemes.FirstOrDefault();
        }

        // When the main window has been loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add extension UI elements
            foreach (var extension in App.LoadedExtensions)
            {
                App.SplashScreen.Message = "Loading extension " + extension.Name;

                try { extension.SetupUI(this); }
                catch (Exception x) {
                    Console.WriteLine("[ERROR] Error loading extension UI: " + x.Message);
                }
            }

            // Initialise code view
            CodeViewer.Initialise();

            // Close splash screen
            App.SplashScreen.Dispatcher.InvokeShutdown();

            // If we have no debugger, open the debugger selector
            // TODO: MOVE ALL THIS!!!
            if (Application.Debugger == null)
            {
                // First try to load from cmd-line args
                string target = null, debugger = null;
                string[] args = Environment.GetCommandLineArgs();

                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i].ToLower().Trim();

                    if ((arg == "-t" || arg == "-target") && (i + 1 != args.Length))
                    {
                        target = args[i + 1];
                    }
                    else if ((arg == "-d" || arg == "-debugger") && (i + 1 != args.Length))
                    {
                        debugger = args[i + 1];
                    }

                    if (target != null && debugger != null) break;
                }

                if (target == null || debugger == null)
                {
                    var dialog = new DebuggerSelector();
                    if (target   != null ) dialog.ImagePath    = target;
                    if (debugger != null)  dialog.DebuggerName = debugger;

                    if (dialog.ShowDialog().Value) // This always has value
                    {
                        target   = dialog.ImagePath;
                        debugger = dialog.DebuggerName;
                    }
                }
                if (target != null && debugger != null)
                {
                    Application.Session = new DebugSession(debugger,
                            App.LoadedExtensions.Select((ex) => ex.Name).ToArray(), target, null);
                    try
                    {
                        App.LoadDebugger(debugger);
                    }
                    catch (Exception x)
                    {
                        var msg = String.Format("An error occurred while loading debugger '{0}':\n{1}", 
                            debugger, x);
                        MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
		
        // Toggles between Maximised and Restored (Normal) window states
		void toggleWindowState()
        {
            if (this.WindowState == WinState.Normal)
            {
                this.WindowState            = WinState.Maximized;
                this.maximiseButton.ToolTip = "Restore Down";
            }
            else
            {
                this.WindowState            = WinState.Normal;
                this.maximiseButton.ToolTip = "Maximise";
            }
		}
		
        // Handler for the titlebar being double-clicked
        private void titlebar_click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.ClickCount == 2) this.toggleWindowState();
        }

        // Handler for mouseover on a titlebar button
        private void OnTitlebarBtnEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = (Grid)sender;
            button.Background.Opacity = 0.45;
        }

        // Handler for the mouse leaving a titlebar button
        private void OnTitlebarBtnLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var button = (Grid)sender;
            button.Background.Opacity = 0;
        }

        // Handler for clicking a titlebar button
        private void OnTitlebarBtnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        	var button = (Grid)sender;
			if (button.Uid == "0") this.Close();
            if (button.Uid == "1") this.WindowState = WinState.Minimized;
			if (button.Uid == "2") this.toggleWindowState();
        }


        /* === IDebugUI API Implementation === */

        public string Theme
        {
            get { return this.currentTheme; }
            set { App.SetTheme(value); }
        }

        public GUIType Type { get { return GUIType.WPF; } }

        // Adds a toolbar panel to the main toolbar
        public void AddToolbarPanel(IToolbarPanel panel) {
            this.toolbarHost.Children.Add((ToolbarPanel)panel);
        }

        // Removes a toolbarl panel from the main toolbar
        public void RemoveToolbarPanel(IToolbarPanel panel) {
            this.toolbarHost.Children.Remove((ToolbarPanel)panel);
        }

        public void RemoveMainPanel(int panelID)
        {
            MainPanel panel = panels[panelID];
            if (panel.Location.Side == PanelSide.Left)
            {
                this.leftTabControl.RemoveTab(panel.GetTabItem());
            }
            else throw new NotImplementedException();
        }
        internal void RemoveMainPanel(MainPanel panel)
        {
            if (panel.Location.Side == PanelSide.Left)
            {
                this.leftTabControl.RemoveTab(panel.GetTabItem());
            }
            else throw new NotImplementedException();
        }

        public void AddMenuItem(string path, IMenuItem item)
        {
            // Ensure item is a WPF Menu Item
            if (item.GetType() != typeof(MenuItem)) {
                throw new IncompatibleObjectException(item);
            }
            else menubar.AddItem(path, (MenuItem)item);
        }

        int lastTabID = 1;
        Dictionary<int, MainPanel> panels = new Dictionary<int,MainPanel>();

        public int AddMainPanel(PanelLocation preferredLocation, IMainPanel panel)
        {
            if (lastTabID == Int32.MaxValue) throw new IndexOutOfRangeException();

            if (panel.GetType() != typeof(MainPanel)) {
                throw new IncompatibleObjectException(panel);
            }

            MainPanel mainPanel = (MainPanel)panel;

            if (preferredLocation.Side == PanelSide.Left)
            {
                this.leftTabControl.AddTab(mainPanel.GetTabItem());
            }
            else throw new NotImplementedException();

            // Use the preferred location to start
            mainPanel.Location = preferredLocation;

            panels.Add(lastTabID, mainPanel);
            return lastTabID++;
        }

        public void FocusMainPanel(int panelID)
        {
            MainPanel panel = panels[panelID];
            leftTabControl.SelectTab(panel.GetTabItem());
        }

        
        public IToolbarPanel NewToolbarPanel() {
            return new ToolbarPanel();
        }
        public IMainPanel NewMainPanel() {
            return new MainPanel();
        }
        public IToolbarItem NewToolbarItem(bool isToggle = false) {
            return new ToolbarItem(isToggle);
        }
        Windows.IToolbarItem Windows.IDebugUI.NewToolbarItem() {
            return new ToolbarItem();
        }
        Windows.IToolbarItem Windows.IDebugUI.NewToolbarItem(bool isToggle) {
            return new ToolbarItem(isToggle);
        }
        public IMenuItem NewMenuItem(bool isToggle = false) {
            return new MenuItem(isToggle) { Foreground = new SolidColorBrush(Colors.Black) };
        }

        public void InvokeMethod(Delegate @delegate, params object[] args)
        {
            this.Dispatcher.Invoke(@delegate, args);
        }
    }
}