//
// Layer.cs
//
// Author:
//       Stephen Kew <stephen.kew@gmail.com>
//
// Copyright (c) 2016 Stephen Kew
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
using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace PaintDotNetAddin.pdn
{
    [Serializable]
    public abstract class Layer
    {
        // can't change these names. They are set by the deserializer
        private int width;
        private int height;
        private LayerProperties properties;

        public int Width { get { return this.width; } }

        public int Height { get { return this.height; } }



        public bool Visible { get{ return properties._visible; } }

        public bool IsBackground { get { return properties._isBackground; } }

        public byte Opacity { get{ return properties._opacity; } }

        public string Name  { get { return properties._name; } }

        public LayerBlendMode BlendMode { get { return properties._blendMode;} }

        [Serializable]
        public class LayerProperties : ISerializable
        {
            public string _name;
            public bool _visible;
            public bool _isBackground;
            public byte _opacity;
            public LayerBlendMode _blendMode;

            public LayerProperties(SerializationInfo info, StreamingContext context)
            {
                this._name = info.GetString("name");
                this._visible = info.GetBoolean("visible");
                this._isBackground = info.GetBoolean("isBackground");
                if (!info.TryGetValue<byte>("opacity", out this._opacity))
                    this._opacity = byte.MaxValue;
                
                if (!info.TryGetValue<LayerBlendMode>("blendMode", out this._blendMode))
                    this._blendMode = LayerBlendMode.Normal;
                
                if (Enum.IsDefined(typeof(LayerBlendMode), (object)this._blendMode))
                    return;
                
                this._blendMode = LayerBlendMode.Normal;

                // userMetadataItems - NameValueCollection
            }


            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException ();
            }
        }
    }
}

