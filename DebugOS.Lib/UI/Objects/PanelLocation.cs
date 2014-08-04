
namespace DebugOS
{
    public struct PanelLocation
    {
        public PanelSide Side { get; private set; }

        public PanelLocation(PanelSide side) : this()
        {
            this.Side = side;
        }
    }
}
