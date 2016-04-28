//
// UserBlendOps.cs
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

namespace PaintDotNetAddin.pdn
{
    [Serializable]
    public class UserBlendOps
    {
        [Serializable]
        public class NormalBlendOp
        { }

        [Serializable]
        public class MultiplyBlendOp
        { }

        [Serializable]
        public class AdditiveBlendOp
        { }

        [Serializable]
        public class ColorBurnBlendOp
        { }

        [Serializable]
        public class ColorDodgeBlendOp
        { }

        [Serializable]
        public class ReflectBlendOp
        { }

        [Serializable]
        public class GlowBlendOp
        { }

        [Serializable]
        public class OverlayBlendOp
        { }

        [Serializable]
        public class DifferenceBlendOp
        { }

        [Serializable]
        public class NegationBlendOp
        { }

        [Serializable]
        public class LightenBlendOp
        { }

        [Serializable]
        public class DarkenBlendOp
        { }

        [Serializable]
        public class ScreenBlendOp
        { }

        [Serializable]
        public class XorBlendOp
        { }
    }
}

