﻿//
// BitmapLayer.cs
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

namespace PaintDotNetAddin.pdn
{
    [Serializable]
    public class BitmapLayer : Layer
    {
        private Surface surface;
        private BitmapLayerProperties properties;

        public Surface Surface
        {
            get {
                return surface;
            }
        }

        [Serializable]
        public class BitmapLayerProperties : ISerializable
        {
            private int opacity;
            //private UserBlendOp blendOp;

            public BitmapLayerProperties(SerializationInfo info, StreamingContext context)
            {
                //this.blendOp = (UserBlendOps) null;
                //                SerializationEntry? entry1 = info.TryGetEntry("blendOp");
                //                if (entry1.HasValue)
                //                    this.blendOp = (UserBlendOp) entry1.Value.Value;
                //                
                this.opacity = -1;
                SerializationEntry? entry2 = info.TryGetEntry("opacity");
                if (!entry2.HasValue)
                    return;
                this.opacity = (int) (byte) entry2.Value.Value;
            }

            #region ISerializable implementation

            public void GetObjectData (SerializationInfo info, StreamingContext context)
            {
                throw new NotImplementedException ();
            }

            #endregion
        }
    }
}

