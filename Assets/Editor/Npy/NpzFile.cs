using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class NpzFile : IDisposable // , IReadOnlyDictionary<string, NpyReader>
{
    private ZipArchive archive;
    
    public NpzFile(Stream stream)
    {
        archive = new ZipArchive(stream);
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
            BinaryReader reader = new BinaryReader(stream);
            return new NpyReader(reader);
        }
        else
        {
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (archive != null) archive.Dispose();
        }
    }
}
