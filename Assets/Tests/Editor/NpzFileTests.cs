
using System.Collections.Generic;
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
        NpzFile npz = NpzFile.OpenRead(@"Assets/Resources/oct_lego.npz");
        foreach (string key in npz.Keys)
        {
            var array = npz[key];
        }
    }
}
