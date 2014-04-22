using System;
using System.Windows;
using System.Windows.Controls;
using WinState = System.Windows.WindowState;

namespace DebugOS
{
	public partial class MainWindow : Window, IDebugUI
	{
        private DragBehaviour drag;

        /// <summary>
        /// Window is hidden until loading complete.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

			// Initialise titlebar buttons
			this.closeButton.Background.Opacity    = 0;
			this.maximiseButton.Background.Opacity = 0;
			this.minimiseButton.Background.Opacity = 0;
			
            // Allow the titlebar to drag the window
            this.drag = new DragBehaviour(this.titlebar, this);

            // Ensure we have a debugger attached
            if (App.Debugger == null) return;

            // Ensure we have the current code displayed
            App.Debugger.Stepped += (s, e) => {
                this.codeView.CodeUnit = App.Debugger.CurrentCodeUnit;
            };
            App.Debugger.RefreshRegister += (s, r) => 
            {
                this.Dispatcher.Invoke((Action)delegate
                {
                    bool add = true;

                    foreach (RegisterControl ctrl in this.registerHost.Children)
                    {
                        if (ctrl.nameText.Text == r.Register.Name)
                        {
                            ctrl.SetValue(r.Value);
                            add = false;
                            break;
                        }
                    }
                    if (add) this.registerHost.Children.Add(new RegisterControl(r.Register.Name, r.Value, r.Register.Width));
                });
            };
        }

        // Add extension UI elements
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var extension in App.LoadedExtensions)
            {
                App.SplashScreen.Message = "Loading extension " + extension.Name;

                try { extension.SetupUI(this, App.Debugger); }
                catch (Exception x) {
                    Console.WriteLine("[ERROR] Error loading extension UI: " + x.Message);
                }
            }

            // Close splash screen
            App.SplashScreen.Dispatcher.InvokeShutdown();
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

        public string Theme
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        // Adds a toolbar panel to the main toolbar
        public void AddToolbarPanel(IToolbarPanel panel) {
            this.toolbarHost.Children.Add((ToolbarPanel)panel);
        }

        // Removes a toolbarl panel from the main toolbar
        public void RemoveToolbarPanel(IToolbarPanel panel) {
            this.toolbarHost.Children.Remove((ToolbarPanel)panel);
        }

        public void RemoveMainPanel(int panelID) {
            throw new NotImplementedException();
        }

        public int AddMainPanel(PanelLocation preferredLocation, UIElement panel) {
            throw new NotImplementedException();
        }

        public IToolbarItem NewToolbarItem() {
            return new ToolbarItem();
        }
        public IToolbarPanel NewToolbarPanel() {
            return new ToolbarPanel();
        }
        public IToolbarItem NewToolbarItem(bool isToggle) {
            return new ToolbarItem(isToggle);
        }
    }
}