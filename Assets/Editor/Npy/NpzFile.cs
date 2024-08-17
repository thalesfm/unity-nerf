using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public class NpzFile : IDisposable
{
    private ZipArchive archive;
    // private Dictionary<string, ZipArchiveEntry> entries;

    public static NpzFile OpenRead(string path)
    {
        var stream = File.OpenRead(path);
        return new NpzFile(stream);
    }

    public NpzFile(Stream stream)
    {
        archive = new ZipArchive(stream);
        // entries = new Dictionary<string, ZipArchiveEntry>();
        // foreach (ZipArchiveEntry entry in archive.Entries) {
        //     entries[entry.FullName] = entry;
        // }
    }

    // HACK: For testing purposes
    public Stream GetArrayStream(string name)
    {
        var entry = archive.GetEntry(name);
        if (entry != null)
        {
            return entry.Open();
        }
        else
        {
            return null;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing)
        {
            if (archive != null) archive.Dispose();
        }
    }
}
