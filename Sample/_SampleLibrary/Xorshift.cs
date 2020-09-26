using System;

namespace SampleLibrary
{
    [ToString]
    public class Xorshift : Random
    {
        private uint x = 123456789;
        private uint y = 362436069;
        private uint z = 521288629;
        private uint w;

        private static readonly Random rnd = new Random();
        public Xorshift() : this(rnd.Next()) { }
        public Xorshift(int seed) { w = (uint)seed; }

        protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue);
        private uint InternalSample()
        {
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
        }
    }
}
