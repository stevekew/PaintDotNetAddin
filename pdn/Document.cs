//
// Document.cs
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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;

namespace PaintDotNetAddin.pdn
{
    [Serializable]
    public class Document : ISerializable
    {
        private static string MAGIC_HEADER_STRING = "PDN3";

        private XmlDocument _headerXml;
        private LayerList _layers;
        private int _width;
        private int _height;

        public XmlDocument HeaderXml { get { return this._headerXml; } set { _headerXml = value; } }

        public LayerList Layers { get { return this._layers; } }

        public int Width { get { return this._width; } }

        public int Height { get { return this._height; } }

        public Document(SerializationInfo info, StreamingContext context)
        {
            this._layers = info.GetValue<LayerList>("layers");
            this._width = info.GetInt32("width");
            this._height = info.GetInt32("height");
            // also:
            // savedWith - Version
            // userMetadataItems - NameValueCollection
        }

        public static byte[] MagicBytes { get { return Encoding.UTF8.GetBytes(MAGIC_HEADER_STRING); } }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException ();
        }
    }
}

