namespace SampleLibrary
{
    public static unsafe class UnsafeBlock
    {
        public static ulong Convert(double d)
        {
            double* p = &d;
#if NET10_0_OR_GREATER
            System.Console.WriteLine(System.Runtime.CompilerServices.Unsafe.BitCast<char, short>('a'));
#endif
            return *(ulong*)p;
        }
    }
}
