namespace SampleLibrary
{
    public static unsafe class UnsafeBlock
    {
        public static ulong Convert(double d)
        {
            double* p = &d;
            return *(ulong*)p;
        }
    }
}
