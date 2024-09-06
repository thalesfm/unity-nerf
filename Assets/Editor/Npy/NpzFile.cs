using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class NpzFile : IDisposable, IReadOnlyDictionary<string, object>
{
    private ZipArchive archive;

    // FIXME: Parses and discards all arrays!
    public IEnumerable<string> Keys => this.Select(e => e.Key);

    public IEnumerable<object> Values => this.Select(e => e.Value);

    public int Count => this.Count();

    public object this[string key]
    {
        get => GetArray(key);
    }

    public NpzFile(Stream stream)
    {
        archive = new ZipArchive(stream);
    }

    public static NpzFile OpenRead(string path)
    {
        var stream = File.OpenRead(path);
        return new NpzFile(stream);
    }

    public object GetArray(string name)
    {
        var entry = archive.GetEntry(name);
        if (entry != null)
        {
            Stream stream = entry.Open();
            BinaryReader reader = new BinaryReader(stream);
            var npy = new NpyReader(reader);
            return npy.ReadArray();
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

    public bool ContainsKey(string key)
    {
        return archive.GetEntry(key) != null;
    }

    public bool TryGetValue(string key, out object value)
    {
        value = GetArray(key);
        return value != null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            object array = GetArray(entry.FullName);
            yield return KeyValuePair.Create(entry.FullName, array);
        }
    }
}
