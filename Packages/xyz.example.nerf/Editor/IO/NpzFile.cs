using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UnityNeRF.Editor.IO
{
    public sealed class NpzFile : IDisposable
    {
        private ZipArchive zip;

        public NpzFile(Stream stream)
        {
            zip = new ZipArchive(stream);
        }

        public NpzFile(Stream stream, bool leaveOpen)
        {
            zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen);
        }

        public void Dispose()
        {
            zip.Dispose();
        }

        public static NpzFile OpenRead(string path)
        {
            var stream = File.OpenRead(path);
            return new NpzFile(stream);
        }

        public IEnumerable<KeyValuePair<string, NpyReader>> Entries
        {
            get
            {
                foreach (var entry in zip.Entries)
                {
                    string name = entry.FullName;
                    if (Path.GetExtension(name) == ".npy")
                    {
                        yield return KeyValuePair.Create(name, GetReader(name));
                    }
                }
            }
        }

        public NpyReader this[string name] => GetReader(name);

        public bool ContainsEntry(string name) => zip.GetEntry(name) != null;

        public Array ReadArray(string name) =>
            GetReader(name).ReadArray();

        public Array ReadArray(string name, out int[] shape) =>
            GetReader(name).ReadArray(out shape);

        public T[] ReadArray<T>(string name) =>
            GetReader(name).ReadArray<T>();

        public T[] ReadArray<T>(string name, out int[] shape) =>
            GetReader(name).ReadArray<T>(out shape);

        public short ReadInt16(string name) => GetReader(name).ReadInt16();
        public int ReadInt32(string name) => GetReader(name).ReadInt32();
        public long ReadInt64(string name) => GetReader(name).ReadInt64();
        public ushort ReadUInt16(string name) => GetReader(name).ReadUInt16();
        public uint ReadUInt32(string name) => GetReader(name).ReadUInt32();
        public ulong ReadUInt64(string name) => GetReader(name).ReadUInt64();
        public Half ReadHalf(string name) => GetReader(name).ReadHalf();
        public float ReadSingle(string name) => GetReader(name).ReadSingle();
        public double ReadDouble(string name) => GetReader(name).ReadDouble();
        public string ReadString(string name) => GetReader(name).ReadString();

        private NpyReader GetReader(string name)
        {
            ZipArchiveEntry entry = zip.GetEntry(name) ?? throw new KeyNotFoundException();
            return new NpyReader(entry.Open());
        }
    }
} // namespace UnityNeRF.Editor.IO
