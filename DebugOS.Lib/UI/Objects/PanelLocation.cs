/* PanelLocation.cs - (c) James S Renwick 2014
 * -------------------------------------------
 * Version 1.1.0
 */
namespace DebugOS
{
    /// <summary>
    /// Data type decribing the placement of a user interface panel.
    /// </summary>
    public struct PanelLocation
    {
        public bool ExternalDialog { get; private set; }
        public bool ExternalWindow { get; private set; }
        public PanelSide Side { get; private set; }

        /// <summary>
        /// Creates a new PanelLocation describing the location of a user
        /// interface panel. 
        /// 
        /// Use <see cref="PanelLocation.Dialog"/> for an external dialog.
        /// </summary>
        /// <param name="side">The side at which the panel is positioned.</param>
        /// <param name="externalWindow">Whether the panel is shown in an external window.</param>
        public PanelLocation(PanelSide side, bool externalWindow = false) : this()
        {
            this.Side           = side;
            this.ExternalWindow = externalWindow;
        }


        /// <summary>
        /// The panel location for an external dialog.
        /// </summary>
        public static readonly PanelLocation Dialog;

        static PanelLocation()
        {
            Dialog = new PanelLocation()
            {
                ExternalDialog = true,
                Side = PanelSide.Any
            };
        }
    }
}
