using System;
using System.IO;
using System.Text;

// #if !NET7_0_OR_GREATER
// using NET_7_0_CompatExtensions;
// #endif

// HACK: For testing purposes only
using NDArray = System.Object;

public static class NpyReader
{
    private static readonly byte[] MAGIC_PREFIX = {147, 78, 85, 77, 80, 89};

    private static NpyVersion ReadMagic(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(8);
        if (magic.Length == 8 && magic.AsSpan(0, 6).SequenceEqual(MAGIC_PREFIX))
        {
            byte major = magic[6];
            byte minor = magic[7];
            return new NpyVersion(major, minor);
        }
        else
        {
            throw new IOException("Failed parse NPY file");
        }
    }

    private static NpyHeader ReadHeader(BinaryReader reader)
    {
        NpyVersion version = ReadMagic(reader);

        int headerLength = version switch {
            { Major: 1, Minor: 0 } => reader.ReadUInt16(),
            { Major: 2, Minor: 0 } => checked((int) reader.ReadUInt32()),
            _ => throw new IOException("Failed to parse NPY file: unsupported version number"),
        };

        var headerBytes = reader.ReadBytes(headerLength);
        var headerStr = Encoding.ASCII.GetString(headerBytes);
        UnityEngine.Debug.Log("Got header: " + headerStr);
        return NpyHeader.Parse(headerStr);
    }

    // TODO
    public static NDArray Read(Stream stream)
    {
        var reader = new BinaryReader(stream);
        NpyHeader header = ReadHeader(reader);
        UnityEngine.Debug.Log("Header: " + header);
        return null;
    }
}
