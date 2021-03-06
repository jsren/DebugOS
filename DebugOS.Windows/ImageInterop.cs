﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DebugOS.Windows
{
    public static partial class Interop
    {
        public static BitmapSource ConvertImage(Bitmap bitmap)
        {
            if (bitmap == null) 
                throw new ArgumentNullException("bitmap");

            using (MemoryStream bitmapStream = new MemoryStream())
            {
                bitmap.Save(bitmapStream, ImageFormat.Png);
                bitmapStream.Seek(0, SeekOrigin.Begin);

                BitmapDecoder decoder = BitmapDecoder.Create
                (
                    bitmapStream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad
                );

                var output = new WriteableBitmap(decoder.Frames.Single());
                output.Freeze();

                return output;
            }
        }

        public static Bitmap ConvertImage(BitmapSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            
            using (MemoryStream bitmapStream = new MemoryStream())
            {
                encoder.Save(bitmapStream);
                bitmapStream.Seek(0, SeekOrigin.Begin);

                var output = new Bitmap(bitmapStream);
                return output;
            }
        }
    }
}
