using System;
using System.IO;
using System.Text;

// #if !NET7_0_OR_GREATER
// using NET_7_0_CompatExtensions;
// #endif

public static class NpyReader
{
    private static readonly byte[] MAGIC_PREFIX = {147, 78, 85, 77, 80, 89};

    private static (int, int) ReadMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(8);
        if (magic.Length == 8 && magic.AsSpan(0, 6).SequenceEqual(MAGIC_PREFIX))
        {
            byte major = magic[6];
            byte minor = magic[7];
            return (major, minor);   
        }
        else
        {
            throw new IOException("Failed parse NPY file");
        }
    }

    public static NpyHeader ReadHeader(BinaryReader reader)
    {
        (int, int) version = ReadMagic(reader);
        int headerLength;

        var str = version switch {
            (1, 0) => "Ok",
            (2, 0) => "Ok",
            _ => "Not ok",
        };

        if (version == (1, 0))
        {
            headerLength = reader.ReadUInt16();
        }
        else if (version == (2, 0))
        {
            headerLength = checked((int) reader.ReadUInt32());
        }
        else
        {
            throw new IOException("Failed to parse NPY file: unsupported version number");
        }

        var headerBytes = reader.ReadBytes(headerLength);
        var headerStr = Encoding.ASCII.GetString(headerBytes);
        UnityEngine.Debug.Log("Got header: " + headerStr);
        return NpyHeader.Parse(headerStr);
    }
}
