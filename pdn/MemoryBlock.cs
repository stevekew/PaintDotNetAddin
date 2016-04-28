//
// MemoryBlock.cs
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
using System.IO;
using System.Globalization;
using System.IO.Compression;

namespace PaintDotNetAddin.pdn
{
    [Serializable]
    public sealed class MemoryBlock : IDeferredSerializable, ISerializable
    {
        private long _length;
        private byte[] _buffer;
        private MemoryBlock _parentBlock;

        [NonSerialized]
        private bool _valid;




        public byte[] Buffer { get { return _buffer; } }

        public MemoryBlock Parent { get { return this._parentBlock; } }

        public long Length { get { return this._length; } }

        private unsafe MemoryBlock(SerializationInfo info, StreamingContext context)
        {
            this._length = !info.ContainsEntry("length64") ? (long) info.GetInt32("length") : info.GetInt64("length64");

            if (info.ContainsEntry("bitmapWidth"))
            {
                int width = info.GetInt32("bitmapWidth");
                int height = info.GetInt32("bitmapHeight");
                if ((width != 0 || height != 0) && (long) width * (long) height * 4L != this._length)
                    throw new ApplicationException("Invalid file format: width * height * 4 != length");
            }

            if (info.GetBoolean("hasParent"))
            {
                this._parentBlock = (MemoryBlock) info.GetValue("parentBlock", typeof (MemoryBlock));
                long parentOffset;
                try
                {
                    parentOffset = info.GetInt64("parentOffset64");
                }
                catch (SerializationException ex)
                {
                    parentOffset = (long) info.GetInt32("parentOffset");
                }
                this._valid = true;
            }
            else
            {
                DeferredDeserializer deferredFormatter = context.Context as DeferredDeserializer;
                bool deferred = false;

                info.TryGetValue ("deferred", out deferred);

                if (deferred && deferredFormatter != null)
                {
                    // set this object up for deferred deserialization
                    this._buffer = new byte[this._length];
                    deferredFormatter.AddDeferredObject(this);
                }
                else if (deferred && deferredFormatter == null)
                {
                    throw new InvalidOperationException("stream has deferred serialization streams, but a DeferredFormatter was not provided");
                }
                else
                {
                    // not deferred, so load
                    this._buffer = new byte[this._length];
                    this._valid = true;
                    bool processed = false;
                    if (info.ContainsEntry("pointerData"))
                    {
                        try
                        {
                            byte[] numArray = (byte[]) info.GetValue("pointerData", typeof (byte[]));
                            Array.Copy(numArray, 0, _buffer, 0, numArray.LongLength);
                            processed = true;
                        }
                        catch (SerializationException ex)
                        {
                            processed = false;
                        }
                    }
                    if (processed)
                        return;
                    
                    uint chunkCount = info.GetUInt32("chunkCount");
                    int chunkFormat = info.GetInt32("chunkFormat");
                    if (chunkFormat != 0)
                        throw new FormatException("Invalid chunkFormat. Expected 0, but got " + chunkFormat.ToString());
                    long num = 0;
                    for (uint index = 0; index < chunkCount; ++index)
                    {
                        string name = "chunk" + index.ToString((IFormatProvider) CultureInfo.InvariantCulture);
                        byte[] numArray = info.GetValue<byte[]>(name);
                        fixed (byte* numPtr = numArray)
                        Array.Copy(numArray, 0, _buffer, num, numArray.LongLength);
                        num += numArray.LongLength;
                    }
                    if (num != this._length)
                        throw new FormatException(string.Format("length={0}, but all the chunks only account for {1} bytes", (object) this._length.ToString("N0"), (object) num.ToString("N0")));
                }
            }
        }

        private static uint ReadUInt(Stream output)
        {
            uint num1 = 0;
            for (int index = 0; index < 4; ++index)
            {
                uint num2 = num1 << 8;
                int num3 = output.ReadByte();
                if (num3 == -1)
                    throw new EndOfStreamException();
                num1 = num2 + (uint) num3;
            }
            return num1;
        }

        private unsafe void DecompressChunk(byte[] compressedBytes, uint chunkSize, long chunkOffset, DeferredDeserializer deferredFormatter)
        {
            using (GZipStream input = new GZipStream((Stream) new MemoryStream(compressedBytes, false), CompressionMode.Decompress, true))
            {
                input.Read(_buffer,(int) chunkOffset,(int) chunkSize);
            }
        }

        public unsafe void FinishDeserialization(Stream input, DeferredDeserializer formatter)
        {
            this._valid = true;
            int chunkFormat = input.ReadByte();
            switch (chunkFormat)
            {
            case -1:
                throw new EndOfStreamException();
            case 0:
                goto case 1;
            case 1:
                {
                    uint chunkSize = MemoryBlock.ReadUInt (input);

                    uint chunkCount = (uint)((ulong)(this._length + (long)chunkSize - 1L) / (ulong)chunkSize);

                    bool[] flagArray = new bool[chunkCount];

                    for (uint index = 0; index < chunkCount; ++index) {
                        uint chunkNumber = MemoryBlock.ReadUInt (input);

                        if (chunkNumber >= chunkCount)
                            throw new SerializationException (string.Format ("chunkNumber ({0}) read from stream is out of bounds (chunkCount = {1})", (object)chunkNumber, (object)chunkCount));

                        if (flagArray [chunkNumber])
                            throw new SerializationException ("already encountered chunk #" + chunkNumber.ToString ());

                        flagArray [chunkNumber] = true;
                        uint readLen = MemoryBlock.ReadUInt (input);
                        long chunkOffset = (long)chunkNumber* (long)chunkSize;
                        uint actualChunkSize = Math.Min (chunkSize, (uint)(this._length - chunkOffset));

                        if (chunkOffset < 0L || chunkOffset >= this._length || chunkOffset + (long)actualChunkSize > this._length)
                            throw new SerializationException ("data was specified to be out of bounds");

                        byte[] numArray = new byte[readLen];
                        input.ProperRead (numArray, 0, numArray.Length);

                        if (chunkFormat == 0)
                        {
                            try
                            {
                                this.DecompressChunk(numArray, actualChunkSize, chunkOffset, formatter);
                            }
                            catch (Exception ex)
                            {
                                throw new SerializationException("Exception thrown by worker thread", ex);
                            }
                        } else 
                        {
                            Array.Copy (numArray, 0, _buffer, chunkOffset, actualChunkSize);
                        }
                    }

                }
                break;
            default:
                throw new SerializationException("formatVersion was neither zero nor one");
            }
        }


        public unsafe void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // not supported
            throw new NotImplementedException ();
        }
    }
}

