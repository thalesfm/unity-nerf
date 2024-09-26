using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UnityNeRF.Editor.IO
{

public sealed class NpzFile : IDisposable
{
    private ZipArchive archive;
    
    public NpzFile(Stream stream)
    {
        archive = new ZipArchive(stream);
    }

    public void Dispose()
    {
        archive.Dispose();
    }

    public static NpzFile OpenRead(string path)
    {
        var stream = File.OpenRead(path);
        return new NpzFile(stream);
    }

    public IEnumerable<KeyValuePair<string, NpyReader>> Arrays()
    {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            NpyReader reader = GetArray(entry.FullName);
            yield return KeyValuePair.Create(entry.FullName, reader);
        }
    }

    public NpyReader GetArray(string name)
    {
        var entry = archive.GetEntry(name);
        if (entry != null)
        {
            Stream stream = entry.Open();
            return new NpyReader(stream);
        }
        else
        {
            return null;
        }
    }
}

} // namespace UnityNeRF.Editor.IO