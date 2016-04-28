//
// PaintDotNetImporter.cs
//
// Author:
//       steve <${AuthorEmail}>
//
// Copyright (c) 2016 steve
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using Gdk;
using Pinta.Core;
using Cairo;
using PaintDotNetAddin.pdn;
using System.Drawing;

namespace PaintDotNetAddin
{
    public class PaintDotNetImporter : Pinta.Core.IImageImporter
    {

        #region IImageImporter implementation

        public void Import (string filename, Gtk.Window parent)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            pdn.Document loaded = PaintDotNetFileLoader.FromStream((stream));


            // Create a new document and add an initial layer.
            Pinta.Core.Document doc = PintaCore.Workspace.CreateAndActivateDocument(filename, new Gdk.Size (loaded.Width, loaded.Height));
            doc.HasFile = true;
            doc.Workspace.CanvasSize = doc.ImageSize;

            int index = 0;
            foreach(BitmapLayer l in loaded.Layers)
            {
                UserLayer layer = doc.CreateLayer(l.Name, l.Width, l.Height);
                doc.Insert(layer, index++);

                layer.Hidden = !l.Visible; // pdn stores visible, not hidden
                layer.BlendMode = ConvertBlendMode(l.BlendMode);
                layer.Opacity = Convert.ToDouble(l.Opacity) / 255.0; // pdn stores opacity with 255 as max

                // Copy over the image data to the layer's surface.
                CopyToSurface (l.Surface.Data.Buffer, layer.Surface);
            }
        }

        private static unsafe void CopyToSurface (byte[] image_data, Cairo.ImageSurface surf)
        {
            if (image_data.Length != surf.Data.Length)
                throw new ArgumentException ("Mismatched image sizes");

            surf.Flush ();

            ColorBgra* dst = (ColorBgra *)surf.DataPtr;
            int len = image_data.Length / ColorBgra.SizeOf;

            fixed (byte *src_bytes = image_data) {
                ColorBgra *src = (ColorBgra *)src_bytes;

                for (int i = 0; i < len; ++i) {

                    ColorBgra srcCurrent = *src;
                    // PDN transparet is 255,255,255,0 Pinta is 0,0,0,0
                    if (srcCurrent.A == 0) {
                        srcCurrent.B = 0;
                        srcCurrent.G = 0;
                        srcCurrent.R = 0;
                    }
                    *dst = srcCurrent;
                    dst++;
                    src++;
                }
            }

            surf.MarkDirty ();
        }

        public Gdk.Pixbuf LoadThumbnail (string filename, int maxWidth, int maxHeight, Gtk.Window parent)
        {
            FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            Pixbuf pb = null;

            byte[] data = PaintDotNetFileLoader.GetThumbnailData (stream);

            if(data != null)
            {
                pb = new Pixbuf (data);
            }
            return pb;
        }

        #endregion

        BlendMode ConvertBlendMode (LayerBlendMode blendMode)
        {
            switch (blendMode) {
            case LayerBlendMode.Normal:
                {
                    return BlendMode.Normal;
                }
            case LayerBlendMode.Multiply:
                {
                    return BlendMode.Multiply;
                }
            case LayerBlendMode.ColorBurn:
                {
                    return BlendMode.ColorBurn;
                }
            case LayerBlendMode.ColorDodge:
                {
                    return BlendMode.ColorDodge;
                }
            case LayerBlendMode.Overlay:
                {
                    return BlendMode.Overlay;
                }
            case LayerBlendMode.Difference:
                {
                    return BlendMode.Difference;
                }

            case LayerBlendMode.Lighten:
                {
                    return BlendMode.Lighten;
                }
            case LayerBlendMode.Darken:
                {
                    return BlendMode.Darken;
                }
            case LayerBlendMode.Screen:
                {
                    return BlendMode.Screen;
                }
            case LayerBlendMode.Xor:
                {
                    return BlendMode.Xor;
                }
                // not supported by pinta
                //            case LayerBlendMode.Reflect:
                //                {
                //                }
                //            case LayerBlendMode.Glow:
                //                {
                //                }
                //            case LayerBlendMode.Negation:
                //                {
                //                }
                //            case LayerBlendMode.Additive:
                //                {
                //                }

            default:
                return BlendMode.Normal;
            }
        }
    }
}

