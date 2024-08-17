public readonly struct NpyVersion
{
    public readonly int Major;
    public readonly int Minor;

    public NpyVersion(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }
}