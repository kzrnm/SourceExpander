using Kzrnm.Numerics;

namespace SampleLibrary.Under
{
    public static class BigInt
    {
        public static byte[] Bytes(BigInteger value)
            => value.ToByteArray();
    }
}
