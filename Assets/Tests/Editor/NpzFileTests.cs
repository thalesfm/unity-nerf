using System;
using System.IO;
using NUnit.Framework;

public class NpzFileTests
{
    [Test]
    public void TestConstructor()
    {
        var stream = File.OpenRead(@"Assets/Resources/oct_lego.npz");
        new NpzFile(stream);
    }

    [Test]
    public void TestGetArray()
    {
        NpzFile npz = NpzFile.OpenRead(@"Assets/Resources/oct_lego.npz");
        foreach ((var name, var reader) in npz.Arrays())
        {
            if (name == "data.npy") continue;
            // if (name == "child.npy") continue;
            var array = reader.ReadArray();
        }
    }

    [Test]
    public void TestGetValue()
    {
        NpzFile npz = NpzFile.OpenRead(@"Assets/Resources/oct_lego.npz");
        NpyReader npy = npz.GetArray("data_dim.npy");
        long[] dataDim = npy.ReadArray<long>();
        UnityEngine.Debug.Log($"data_dim: {dataDim[0]}");
        npy = npz.GetArray("data_format.npy");
        string[] dataFormat = npy.ReadArray<string>();
        UnityEngine.Debug.Log($"data_format: {dataFormat[0]}");
    }
}
