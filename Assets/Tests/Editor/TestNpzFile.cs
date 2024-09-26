using System;
using System.IO;
using NUnit.Framework;

public class TestNpzFile
{
    [Test]
    public void TestGetArray()
    {
        NpzFile npz = NpzFile.OpenRead(@"Assets/Resources/oct_lego.npz");
        foreach ((var name, var reader) in npz.Arrays())
        {
            // if (name == "data.npy") continue;
            // if (name == "child.npy") continue;
            var array = reader.ReadArray();
        }
    }

    // [Test]
    // public static void TestGetData()
    // {
    //     using NpzFile npz = NpzFile.OpenRead(@"Assets/Resources/oct_lego.npz");
    //     NpyReader npy = npz.GetArray("data.npy");
    //     Half[] data = npy.ReadArray<Half>();
    //     Random rnd = new Random();
    //     for (int k = 0; k < 10; ++k)
    //     {
    //         int i = rnd.Next(data.Length);
    //         Half value = data[i];
    //         UnityEngine.Debug.Log($"data[{i}]: {value}");
    //     }
    // }

    // [Ignore("Still unoptimized; very slow")]
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
