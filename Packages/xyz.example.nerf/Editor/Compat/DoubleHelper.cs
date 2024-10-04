internal class DoubleHelper
{
    //
    // Constants for manipulating the private bit-representation
    //

    internal const ulong SignMask = 0x8000_0000_0000_0000;
    internal const int SignShift = 63;
    internal const byte ShiftedSignMask = (byte)(SignMask >> SignShift);

    internal const ulong BiasedExponentMask = 0x7FF0_0000_0000_0000;
    internal const int BiasedExponentShift = 52;
    internal const int BiasedExponentLength = 11;
    internal const ushort ShiftedExponentMask = (ushort)(BiasedExponentMask >> BiasedExponentShift);

    internal const ulong TrailingSignificandMask = 0x000F_FFFF_FFFF_FFFF;

    internal const byte MinSign = 0;
    internal const byte MaxSign = 1;

    internal const ushort MinBiasedExponent = 0x0000;
    internal const ushort MaxBiasedExponent = 0x07FF;

    internal const ushort ExponentBias = 1023;

    internal const short MinExponent = -1022;
    internal const short MaxExponent = +1023;

    internal const ulong MinTrailingSignificand = 0x0000_0000_0000_0000;
    internal const ulong MaxTrailingSignificand = 0x000F_FFFF_FFFF_FFFF;

    internal const int TrailingSignificandLength = 52;
    internal const int SignificandLength = TrailingSignificandLength + 1;
}