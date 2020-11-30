using System.Diagnostics;

namespace SampleLibrary
{
    public static class Put
    {
        private static readonly Xorshift rnd = new Xorshift();
        public static void WriteRandom() { Trace.WriteLine(rnd.Next()); }
    }
}
