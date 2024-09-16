internal class SingleHelper
{
    //
    // Constants for manipulating the private bit-representation
    //

    internal const uint SignMask = 0x8000_0000;
    internal const int SignShift = 31;
    internal const byte ShiftedSignMask = (byte)(SignMask >> SignShift);

    internal const uint BiasedExponentMask = 0x7F80_0000;
    internal const int BiasedExponentShift = 23;
    internal const int BiasedExponentLength = 8;
    internal const byte ShiftedBiasedExponentMask = (byte)(BiasedExponentMask >> BiasedExponentShift);

    internal const uint TrailingSignificandMask = 0x007F_FFFF;

    internal const byte MinSign = 0;
    internal const byte MaxSign = 1;

    internal const byte MinBiasedExponent = 0x00;
    internal const byte MaxBiasedExponent = 0xFF;

    internal const byte ExponentBias = 127;

    internal const sbyte MinExponent = -126;
    internal const sbyte MaxExponent = +127;

    internal const uint MinTrailingSignificand = 0x0000_0000;
    internal const uint MaxTrailingSignificand = 0x007F_FFFF;

    internal const int TrailingSignificandLength = 23;
    internal const int SignificandLength = TrailingSignificandLength + 1;
}