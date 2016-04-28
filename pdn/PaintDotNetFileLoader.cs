//
// PaintDotNetFileLoader.cs
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
using System.IO;
using System.Xml;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.IO.Compression;
using System;
using System.Collections.Generic;

namespace PaintDotNetAddin.pdn
{
    public static class PaintDotNetFileLoader
    {
        private static int GZIP_HEADER_BYTE1 = 31;
        private static int GZIP_HEADER_BYTE2 = 139;

        private static int DEFERRED_HEADER_BYTE1 = 0;
        private static int DEFERRED_HEADER_BYTE2 = 1;


        public static byte[] GetThumbnailData(Stream stream) 
        {
            byte[] data = null;

            long streamStartPos = stream.Position;
            bool hasXmlHeader = true;

            // check the file header
            for (int index = 0; index < Document.MagicBytes.Length; ++index)
            {
                int num = stream.ReadByte();
                if (num == -1)
                    throw new EndOfStreamException();

                if (num != (int)Document.MagicBytes[index])
                {
                    hasXmlHeader = false;
                    break;
                }
            }

            XmlDocument xmlDocument = (XmlDocument)null;
            if (hasXmlHeader) 
            {
                int num1 = stream.ReadByte ();
                if (num1 == -1)
                    throw new EndOfStreamException ();
                int num2 = stream.ReadByte ();
                if (num2 == -1)
                    throw new EndOfStreamException ();
                int num3 = stream.ReadByte ();
                if (num3 == -1)
                    throw new EndOfStreamException ();
                int count = num1 + (num2 << 8) + (num3 << 16);
                byte[] numArray = new byte[count];
                int num4 = stream.ProperRead (numArray, 0, count);
                if (num4 != count)
                    throw new EndOfStreamException ("expected " + (object)count + " bytes, but only got " + (object)num4);
                string @string = Encoding.UTF8.GetString (numArray);
                xmlDocument = new XmlDocument ();
                xmlDocument.LoadXml (@string);

                XmlElement thumbnailElement = xmlDocument.SelectSingleNode ("//pdnImage/custom/thumb") as XmlElement;

                if(thumbnailElement != null)
                {
                    if (thumbnailElement.HasAttribute ("png"))
                    {
                        data = System.Convert.FromBase64String (thumbnailElement.GetAttribute("png"));
                    }
                }
            } 

            return data;
        }

        public static Document FromStream(Stream stream)
        {
            long streamStartPos = stream.Position;
            bool hasXmlHeader = true;

            // check the file header
            for (int index = 0; index < Document.MagicBytes.Length; ++index)
            {
                int num = stream.ReadByte();
                if (num == -1)
                    throw new EndOfStreamException();
                
                if (num != (int)Document.MagicBytes[index])
                {
                    hasXmlHeader = false;
                    break;
                }
            }


            XmlDocument xmlDocument = (XmlDocument)null;
            if (hasXmlHeader) 
            {
                int num1 = stream.ReadByte ();
                if (num1 == -1)
                    throw new EndOfStreamException ();
                int num2 = stream.ReadByte ();
                if (num2 == -1)
                    throw new EndOfStreamException ();
                int num3 = stream.ReadByte ();
                if (num3 == -1)
                    throw new EndOfStreamException ();
                int count = num1 + (num2 << 8) + (num3 << 16);
                byte[] numArray = new byte[count];
                int num4 = stream.ProperRead (numArray, 0, count);
                if (num4 != count)
                    throw new EndOfStreamException ("expected " + (object)count + " bytes, but only got " + (object)num4);
                string @string = Encoding.UTF8.GetString (numArray);
                xmlDocument = new XmlDocument ();
                xmlDocument.LoadXml (@string);
            } 
            else 
            {
                // no XML header, so go back to the start of the stream
                stream.Position = streamStartPos;
            }
            
            long dataStartPos = stream.Position;

            int dataByte1 = stream.ReadByte();

            if (dataByte1 == -1)
                throw new EndOfStreamException();
            
            int dataByte2 = stream.ReadByte();

            if (dataByte2 == -1)
                throw new EndOfStreamException();

            // we're not at the end of the stream, so setup the deserializer
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            // use our custom binder to convert from the Paint.net classes to out internal classes
            var serializationBinder = GetCustomSerializationBinder ();

            serializationBinder.SetNextRequiredBaseType (typeof(Document));

            binaryFormatter.Binder = (SerializationBinder)serializationBinder;

            Document document;
            if ((dataByte1 == DEFERRED_HEADER_BYTE1) && (dataByte2 == DEFERRED_HEADER_BYTE2)) 
            {
                //use deferred serialization - it means the file data is not sequential

                DeferredDeserializer deferredDeserializer = new DeferredDeserializer();
                binaryFormatter.Context = new StreamingContext(binaryFormatter.Context.State, deferredDeserializer);

                // deserialize the document
                document = (Document)binaryFormatter.UnsafeDeserialize(stream, null);

                // then complete the deserialization
                deferredDeserializer.FinishDeserialization(stream);
            }
            else
            {
                // otherwise we can directly deserialise the date from a GZip stream
                // if it matches the correct header bytes
                if (dataByte1 != GZIP_HEADER_BYTE1 || dataByte2 != GZIP_HEADER_BYTE2)
                {
                    throw new FormatException ("File is not a valid Paint.net document");
                }
                
                stream.Position = dataStartPos;
                using (GZipStream gzipStream = new GZipStream (stream, CompressionMode.Decompress, true)) 
                {
                    document = (Document)binaryFormatter.UnsafeDeserialize ((Stream)gzipStream, (HeaderHandler)null);
                }
            }
                
            document.HeaderXml = xmlDocument;
            return document;
        }

        static CustomSerializationBinder GetCustomSerializationBinder ()
        {
            CustomSerializationBinder serializationBinder = new CustomSerializationBinder ();
            serializationBinder.AddCustomType ("PaintDotNet.Document", typeof(Document));
            serializationBinder.AddCustomType ("PaintDotNet.LayerList", typeof(LayerList));
            serializationBinder.AddCustomType ("PaintDotNet.BitmapLayer", typeof(BitmapLayer));
            serializationBinder.AddCustomType ("PaintDotNet.BitmapLayer+BitmapLayerProperties", typeof(BitmapLayer.BitmapLayerProperties));
            serializationBinder.AddCustomType ("PaintDotNet.Layer", typeof(Layer));
            serializationBinder.AddCustomType ("PaintDotNet.Layer+LayerProperties", typeof(Layer.LayerProperties));
            serializationBinder.AddCustomType ("PaintDotNet.Surface", typeof(Surface));

            serializationBinder.AddCustomType ("PaintDotNet.LayerBlendMode", typeof(LayerBlendMode));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps", typeof(UserBlendOps));

            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+NormalBlendOp", typeof(UserBlendOps.NormalBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+MultiplyBlendOp", typeof(UserBlendOps.MultiplyBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+AdditiveBlendOp", typeof(UserBlendOps.AdditiveBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+ColorBurnBlendOp", typeof(UserBlendOps.ColorBurnBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+ColorDodgeBlendOp", typeof(UserBlendOps.ColorDodgeBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+ReflectBlendOp", typeof(UserBlendOps.ReflectBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+GlowBlendOp", typeof(UserBlendOps.GlowBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+OverlayBlendOp", typeof(UserBlendOps.OverlayBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+DifferenceBlendOp", typeof(UserBlendOps.DifferenceBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+NegationBlendOp", typeof(UserBlendOps.NegationBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+LightenBlendOp", typeof(UserBlendOps.LightenBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+DarkenBlendOp", typeof(UserBlendOps.DarkenBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+ScreenBlendOp", typeof(UserBlendOps.ScreenBlendOp));
            serializationBinder.AddCustomType ("PaintDotNet.UserBlendOps+XorBlendOp", typeof(UserBlendOps.XorBlendOp));

            serializationBinder.AddCustomType ("PaintDotNet.MemoryBlock", typeof(MemoryBlock));

            return serializationBinder;
        }
    }
}
