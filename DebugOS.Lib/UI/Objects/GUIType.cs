
namespace DebugOS
{
    /// <summary>
    /// The types of graphical user interface under which
    /// the application can be run.
    /// </summary>
    public enum GUIType
    {
        /// <summary>
        /// The application is running in a command-line interface.
        /// Graphical components are not supported.
        /// </summary>
        Console,
        /// <summary>
        /// The application is running the Mono cross-platform
        /// implementaion of WinForms.
        /// </summary>
        MonoForms,
        /// <summary>
        /// The application is running the Windows-specific
        /// implementaion of WinForms.
        /// </summary>
        WinForms,
        /// <summary>
        /// The application is using Windows Presentation
        /// Foundation technologies to display the UI.
        /// </summary>
        WPF
    }
}
