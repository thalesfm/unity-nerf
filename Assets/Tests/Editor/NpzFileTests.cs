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
        foreach (string key in npz.Keys)
        {
            // UnityEngine.Debug.Log($"Reading {key}");
            var array = npz[key];
        }
    }
}
