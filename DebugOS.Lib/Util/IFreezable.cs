
namespace DebugOS
{
    /// <summary>
    /// Describes an object which can enter an irreversible "frozen" state -
    /// a state where previously writable members become read-only.
    /// </summary>
    public interface IFreezable
    {
        /// <summary>
        /// Gets whether the object is in a "frozen" state.
        /// </summary>
        bool IsFrozen { get; }
        /// <summary>
        /// Freezes the object, making members read-only.
        /// </summary>
        void Freeze();
    }
}
