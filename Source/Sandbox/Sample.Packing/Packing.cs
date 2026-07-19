namespace Sample
{
    class P
    {
        public static int Num =>
#if NET10_0
            10
#elif NET9_0
        9
#elif NET8_0
        8
#elif NET7_0
        7
#elif NET6_0
        6
#elif NET5_0
        5
#elif NETSTANDARD2_1
        1
#elif NETSTANDARD2_0
        0
#endif
            ;
    }
}
