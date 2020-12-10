using System.Diagnostics;

namespace SampleLibrary
{
    [DebuggerDisplay("")]
    public static class Put
    {
        private static readonly Xorshift rnd = new Xorshift();
        [DebuggerNonUserCode]
        public static void WriteRandom() { Trace.WriteLine(rnd.Next()); }
    }
}
