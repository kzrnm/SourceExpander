using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace SampleLibrary
{
    public static class Bit
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractLowestSetBit(int n)
        {
            if (Bmi1.IsSupported)
            {
                return (int)Bmi1.ExtractLowestSetBit((uint)n);
            }
            return n & -n;
        }
    }
}
