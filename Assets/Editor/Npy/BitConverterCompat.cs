using System;

internal static class BitConverterCompat
{
    public static Half Int16BitsToHalf(short value)
    {
        throw new NotImplementedException();
    }

    public static Half ToHalf(ReadOnlySpan<byte> value)
    {
        throw new NotImplementedException();
    }

    public static Half ToHalf(byte[] value, int startIndex)
    {
        throw new NotImplementedException();
    }

    public static Half UInt16BitsToHalf(ushort value)
    {
        return new Half(value);
    }
}