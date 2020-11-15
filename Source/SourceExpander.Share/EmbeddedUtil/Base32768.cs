using System;
using System.Collections.Generic;
using System.Text;
/**
Base32768 is a binary-to-text encoding optimised for UTF-16-encoded text.
(e.g. Windows, Java, JavaScript)

original is https://github.com/qntm/base32768

MIT License

Copyright (c) 2020 naminodarie

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
namespace Kzrnm.Convert.Base32768
{
    public static class Base32768
    {
        internal const int BITS_PER_CHAR = 15;// Base32768 is a 15-bit encoding
        internal const int BITS_PER_BYTE = 8;
        internal static readonly (int numZBits, char z)[] lookupD
            = new (int numZBits, char z)[0xA860];
        internal static readonly char[] lookupE7 = Build(new (char from, char to)[] {
            ('\u0180', '\u01a0'), ('\u0240', '\u02a0'),
        }, 128, 1);
        internal static readonly char[] lookupE15 = Build(new (char from, char to)[] {
            ('\u04a0', '\u04c0'), ('\u0500', '\u0520'), ('\u0680', '\u06c0'), ('\u0760', '\u07a0'),
            ('\u07c0', '\u07e0'), ('\u1000', '\u1020'), ('\u10a0', '\u10c0'), ('\u1100', '\u1160'),
            ('\u1180', '\u11a0'), ('\u11e0', '\u1240'), ('\u1260', '\u1280'), ('\u12e0', '\u1300'),
            ('\u1320', '\u1340'), ('\u13a0', '\u13e0'), ('\u1420', '\u1660'), ('\u16a0', '\u16e0'),
            ('\u1780', '\u17a0'), ('\u1820', '\u1860'), ('\u18c0', '\u18e0'), ('\u1980', '\u19a0'),
            ('\u19e0', '\u1a00'), ('\u1a20', '\u1a40'), ('\u1bc0', '\u1be0'), ('\u1c00', '\u1c20'),
            ('\u1d00', '\u1d20'), ('\u21e0', '\u2200'), ('\u22c0', '\u22e0'), ('\u2340', '\u23e0'),
            ('\u2400', '\u2420'), ('\u2500', '\u2760'), ('\u2780', '\u27c0'), ('\u2800', '\u2980'),
            ('\u29a0', '\u29c0'), ('\u2a20', '\u2a60'), ('\u2a80', '\u2ac0'), ('\u2ae0', '\u2b60'),
            ('\u2c00', '\u2c20'), ('\u2c80', '\u2ce0'), ('\u2d00', '\u2d20'), ('\u2d40', '\u2d60'),
            ('\u2ea0', '\u2ee0'), ('\u31c0', '\u31e0'), ('\u3400', '\u4da0'), ('\u4dc0', '\u9fc0'),
            ('\ua000', '\ua480'), ('\ua4a0', '\ua4c0'), ('\ua500', '\ua600'), ('\ua640', '\ua660'),
            ('\ua6a0', '\ua6e0'), ('\ua700', '\ua760'), ('\ua780', '\ua7a0'), ('\ua840', '\ua860'),
        }, 32768, 0);
        private static char[] Build((char from, char to)[] pairString, int size, int r)
        {
            var numZBits = BITS_PER_CHAR - BITS_PER_BYTE * r;
            var encodeRepertoire = new char[size];
            var ix = 0;
            foreach (var (from, to) in pairString)
                for (char i = from; i < to; i++)
                {
                    lookupD[i] = (numZBits, (char)ix);
                    encodeRepertoire[ix++] = i;
                }
            System.Diagnostics.Debug.Assert(size == ix);
            return encodeRepertoire;
        }

        public static string Encode(byte[] bytes)
        {
            var sb = new StringBuilder((BITS_PER_BYTE * bytes.Length + (BITS_PER_CHAR - 1)) / BITS_PER_CHAR);

            const int mask = (1 << 15) - 1;
            var z = 0;
            var numOBits = BITS_PER_CHAR;
            foreach (var by in bytes)
            {
                if (numOBits > 8)
                {
                    numOBits -= 8;
                    z |= by << numOBits;
                }
                else
                {
                    z |= by >> (8 - numOBits);
                    sb.Append(lookupE15[z]);
                    numOBits += 7;
                    z = (by << numOBits) & mask;
                }
            }
            if (numOBits != BITS_PER_CHAR)
            {
                var numZBits = BITS_PER_CHAR - numOBits;
                var c = 7 ^ (numZBits & 0b111);
                if (numZBits > 7)
                {
                    z |= (1 << c) - 1;
                    sb.Append(lookupE15[z]);
                }
                else
                {
                    z >>= 8;
                    z |= (1 << c) - 1;
                    sb.Append(lookupE7[z]);
                }
            }

            return sb.ToString();
        }

        public static byte[] Decode(string str)
        {
            var length = str.Length * BITS_PER_CHAR / BITS_PER_BYTE;
            if (length == 0)
                return Array.Empty<byte>();

            if (str[str.Length - 1] < 1184)
                --length;
            var res = new byte[length];
            var numUint8s = 0;
            var numUint8Remaining = 8;

            for (int i = 0; i < str.Length; i++)
            {
                var chr = str[i];

                var (numZBits, z) = lookupD[chr];
                switch (numZBits)
                {
                    case 15:
                        break;
                    case 7:
                        if (i != str.Length - 1)
                            throw new FormatException($"Unrecognised Base32768 character: {chr}");
                        break;
                    default:
                        throw new FormatException($"Unrecognised Base32768 character: {chr}");
                }

                do
                {
                    var mask = (1 << numZBits) - 1;
                    var zz = z & mask;
                    if (numZBits < numUint8Remaining)
                    {
                        numUint8Remaining -= numZBits;
                        res[numUint8s] |= (byte)(zz << numUint8Remaining);
                        numZBits = 0;
                    }
                    else
                    {
                        numZBits -= numUint8Remaining;
                        res[numUint8s++] |= (byte)(zz >> numZBits);
                        numUint8Remaining = 8;
                    }
                } while (numZBits > 0 && numUint8s < res.Length);
            }

            return res;
        }
    }
}
