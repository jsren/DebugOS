/* MainWindow.xaml.cs - (c) James S Renwick 2014
 * ---------------------------------------------
 * Version 1.5.4
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

        private SmartContextBehaviour contextPopup;

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

            // Add the smart context popup control
            this.contextPopup = new SmartContextBehaviour(this);
            this.contextPopup.Handlers.Add(new AddressContextHandler());

            // Add initial top-level menu items
            menubar.AddItem("", new MenuItem() { Header = "_Debug" });
            menubar.AddItem("", new MenuItem() { Header = "_Settings" });
            menubar.AddItem("", new MenuItem() { Header = "_Help" });
			// Add intitial second-level menu items
            var configItem = new MenuItem() { Header = "Configure DebugOS..."};
            configItem.Clicked += (_) => new ConfigurationDialog().ShowDialog();

			menubar.AddItem("Settings", configItem);

            // Set the current theme - the built-in will be the first
            this.currentTheme = App.LoadedThemes.FirstOrDefault();

            // Handle a null session - prompt for new
            Application.SessionChanged += () =>
            {
                if (Application.Session == null) this.OnNewSession();
            };
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
                string target   = Application.Arguments["-t"] ?? Application.Arguments["-target"];
                string debugger = Application.Arguments["-d"] ?? Application.Arguments["-debugger"];
                string session  = Application.Arguments["-s"] ?? Application.Arguments["-session"];

                // Load from a previous session
                if (session != null)
                {
                    try 
                    {
                        Application.Session = DebugSession.Load(session);

                        target   = Application.Session.ImageFilepath;
                        debugger = Application.Session.Debugger;
                    }
                    catch (Exception x) {
                        MessageBox.Show("Error loading previous session: " + x.ToString());
                    }
                }
                // Otherwise, create new
                else this.OnNewSession(target, debugger);
            }
        }

        void OnNewSession(string target = null, string debugger = null, string architecture = null)
        {
            // Clear the UI
            var ids = panels.Keys.ToArray();

            for (int i = 0; i < ids.Length; i++)
            {
                this.RemoveMainPanel(ids[i]);
            }

            // If not present, prompt the user via the GUI
            if (target == null || debugger == null || architecture == null)
            {
                var dialog = new NewSessionDialog();

                if (target       != null ) dialog.ImagePath    = target;
                if (debugger     != null)  dialog.DebuggerName = debugger;
                if (architecture != null)  dialog.Architecture = architecture;

                if (dialog.ShowDialog().Value) // This always has value
                {
                    target       = dialog.ImagePath;
                    debugger     = dialog.DebuggerName;
                    architecture = dialog.Architecture;
                }
            }

            // Note that target can be empty
            if (target != null && debugger != null && architecture != null)
            {
                var list = App.LoadedArchitectures.Where((arch) => arch.ID == architecture);

                if (list.Count() == 0) throw new Exception("Unknown architecture: " + architecture);

                // Create a new session
                Application.Session = new DebugSession
                (
                    debugger,
                    App.LoadedExtensions.Select((ex) => ex.Name).ToArray(), 
                    target,
                    list.First()
                );
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
            // Update the panel
            MainPanel panel = panels[panelID];
            panel.IsOpen    = false;

            // Remove the corresponding tab
            if (panel.Location.Side == PanelSide.Left)
            {
                this.leftTabControl.RemoveTab(panel.GetTabItem());
            }
            else if (panel.Location.Side == PanelSide.Right)
            {
                this.rightTabControl.RemoveTab(panel.GetTabItem());
            }
            else throw new NotImplementedException();

            // De-register the ID
            this.panels.Remove(panelID);
        }
        internal void RemoveMainPanel(MainPanel panel)
        {
            // Get the panel's ID
            var pairs = this.panels.Where((pair) => pair.Value == panel);

            if (pairs.Count() == 0)
            {
                return;
            }
            else if (pairs.Count() != 1)
            {
                throw new Exception("A MainPanel has been registered more than once with different IDs");
            }
            else this.RemoveMainPanel(pairs.First().Key);
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

            mainPanel.Closed += (sender) =>
            {
                this.RemoveMainPanel((MainPanel)sender);
            };

            if (preferredLocation.Side == PanelSide.Left)
            {
                this.leftTabControl.AddTab(mainPanel.GetTabItem());
            }
            else if (preferredLocation.Side == PanelSide.Right)
            {
                this.rightTabControl.AddTab(mainPanel.GetTabItem());
            }
            else throw new NotImplementedException();

            // Use the preferred location to start
            mainPanel.Location = preferredLocation;

            panels.Add(lastTabID, mainPanel);
            return lastTabID++;
        }

        public void FocusMainPanel(int panelID)
        {
            MainPanel     panel    = panels[panelID];
            PanelLocation location = panel.Location;

            if (location.Side == PanelSide.Left)
            {
                leftTabControl.SelectTab(panel.GetTabItem());
            }
            else if (location.Side == PanelSide.Right)
            {
                rightTabControl.SelectTab(panel.GetTabItem());
            }
            else throw new NotImplementedException();
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