
using System.IO;
using NUnit.Framework;
using Sprache;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class NpzFileTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestConstructor()
    {
        var stream = File.OpenRead(@"Assets/Resources/oct_lego.npz");
        new NpzFile(stream);
    }

    [Test]
    public void TestGetArray()
    {
        var npz = NpzFile.OpenRead(@"Assets/Resources/oct_lego.npz");
        var arrStream = npz.GetArrayStream("child.npy");
        var arrBinReader = new BinaryReader(arrStream);
        NpyHeader header = NpyReader.ReadHeader(arrBinReader);
        Debug.Log("Header: " + header);
    }
}
