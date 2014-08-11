using System;

namespace DebugOS
{
    public sealed class ImageEventArgs : EventArgs
    {
        public ObjectCodeFile Image { get; private set; }

        public ImageEventArgs(ObjectCodeFile image)
        {
            this.Image = image;
        }
    }
}
