//
// SerializationFallbackBinder.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;

namespace PaintDotNetAddin.pdn
{
    public sealed class CustomSerializationBinder : SerializationBinder
    {
        private Type _nextRequiredBaseType;
        private Dictionary<string, Type> _customTypeMap;

        public CustomSerializationBinder()
        {
            this._customTypeMap = new Dictionary<string, Type> ();
        }

        public void AddCustomType(string originalType, Type replacementType)
        {
            this._customTypeMap.Add (originalType, replacementType);
        }

        public void SetNextRequiredBaseType(Type type)
        {
            this._nextRequiredBaseType = type;
        }

        private bool IsTypeAllowed(string assemblyName, string typeName)
        {
            return !typeName.Equals("System.CodeDom.Compiler.TempFileCollection") && !typeName.Equals("System.IO.FileInfo") && (!typeName.Equals("System.IO.DirectoryInfo") && !typeName.Contains("IWbemClassObjectFreeThreaded"));
        }

        private bool IsTypeAllowed(Type type)
        {
            return !typeof(TempFileCollection).IsAssignableFrom(type) && !typeof(FileSystemInfo).IsAssignableFrom(type);
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            // we ignore the assembly name and return our own custom type

            Console.WriteLine(typeName);

            if (!this.IsTypeAllowed(assemblyName, typeName))
                throw new FormatException("Invalid Type: " + typeName);
           
            Type type = null;

            if (type == (Type)null)
            {
                string typeName1 = typeName + ", " + assemblyName;
                try
                {
                    type = Type.GetType(typeName1, false, true);
                }
                catch (FileLoadException)
                {
                    type = (Type)null;
                }
            }

            if (type == (Type)null) {
                if (!_customTypeMap.ContainsKey (typeName)) {
                    throw new FormatException ("Invalid type: " + typeName);
                }

                type = _customTypeMap [typeName];
            }
                
            if (this._nextRequiredBaseType != (Type)null)
            {
                if (!this._nextRequiredBaseType.IsAssignableFrom(type))
                    throw new InvalidCastException();
                this._nextRequiredBaseType = (Type)null;
            }
                
            return type;
        }


    }
}

