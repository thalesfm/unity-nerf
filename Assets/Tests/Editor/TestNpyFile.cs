using System;
using NUnit.Framework;
using NumSharp;
using UnityNeRF.Editor.IO;

namespace UnityNeRF.Editor.Tests
{

public class TestNpyReader
{
    [TestCase(@"Assets/Tests/Editor/Data/arange_f4.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_f8.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i1.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i2.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i4.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i8.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u1.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u2.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u4.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u8.npy")]
    public static void ReadArray_Arange(string path)
    {
        using NpyReader reader = NpyFile.OpenRead(path);
        Array arr = reader.ReadArray();

        for (int i = 0; i < arr.Length; i++)
        {
            Assert.That(arr.GetValue(i), Is.EqualTo(i));
        }
    }

    [TestCase(@"Assets/Tests/Editor/Data/arange_f2.npy")]
    public static void ReadArray_ArangeOfHalf(string path)
    {
        using NpyReader reader = NpyFile.OpenRead(path);
        Array arr = reader.ReadArray();

        for (int i = 0; i < arr.Length; i++)
        {
            Half expected = (Half)i;
            Assert.That(arr.GetValue(i), Is.EqualTo(expected));
        }
    }

    [TestCase(@"Assets/Tests/Editor/Data/hello_U5.npy")]
    public static void ReadArray_HelloWorld(string path)
    {
        using NpyReader reader = NpyFile.OpenRead(path);
        string[] arr = reader.ReadArray<string>();

        Assert.That(arr[0], Is.EqualTo("Hello"));
        Assert.That(arr[1], Is.EqualTo("World"));
    }

    [TestCase(@"Assets/Tests/Editor/Data/mgrid_i4.npy")]
    // [TestCase(@"Assets/Tests/Editor/Data/mgrid_i4_fortran_order.npy")] // Ignore: fortran order not supported
    public static void ReadArray_Mgrid(string path)
    {
        using NpyReader reader = NpyFile.OpenRead(path);
        NDArray arr = reader.Read();

        for (int i = 0; i < arr.shape[0]; i++)
        {
            for (int j = 0; j < arr.shape[1]; j++)
            {
                Assert.That(arr.GetValue(0, i, j), Is.EqualTo(i));
                Assert.That(arr.GetValue(1, i, j), Is.EqualTo(j));
            }
        }
    }

    [TestCase(@"Assets/Tests/Editor/Data/scalar_b1.npy", false)]
    [TestCase(@"Assets/Tests/Editor/Data/scalar_i4.npy", 42)]
    public static void ReadArray_Scalar<T>(string path, T expected)
    {
        using NpyReader reader = NpyFile.OpenRead(path);
        Array arr = reader.ReadArray();

        Assert.That(arr.GetValue(0), Is.EqualTo(expected));
    }
}

} // namespace UnityNeRF.Editor.Tests