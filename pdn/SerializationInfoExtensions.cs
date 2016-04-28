//
// SerializationInfoExtensions.cs
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
    public static class SerializationInfoExtensions
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        public static bool TryGetValue<T>(this SerializationInfo info, string name, out T value)
        {
            if (info.ContainsEntry(name))
            {
                value = info.GetValue<T>(name);
                return true;
            }
            value = default(T);
            return false;
        }

        public static bool ContainsEntry(this SerializationInfo info, string name)
        {
            return info.TryGetEntry(name).HasValue;
        }

        public static SerializationEntry? TryGetEntry(this SerializationInfo info, string name)
        {
            foreach (SerializationEntry serializationEntry in info)
            {
                if (string.Equals(serializationEntry.Name, name, StringComparison.InvariantCulture))
                    return new SerializationEntry?(serializationEntry);
            }
            return new SerializationEntry?();
        }
    }
}

