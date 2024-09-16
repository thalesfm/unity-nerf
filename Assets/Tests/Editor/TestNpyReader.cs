using System;
using System.IO;
using NUnit.Framework;

// namespace MyCompany.MyPackage.Editor.Tests
// {

public class TestNpyReader
{
    // [TestCase(@"Assets/Tests/Editor/Data/arange_b1.npy", typeof(bool))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_f4_be.npy", typeof(float))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_f4_le.npy", typeof(float))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_f8_be.npy", typeof(double))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_f8_le.npy", typeof(double))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i1.npy", typeof(sbyte))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i2_be.npy", typeof(short))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i2_le.npy", typeof(short))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i4_be.npy", typeof(int))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i4_le.npy", typeof(int))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i8_le.npy", typeof(long))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_i8_be.npy", typeof(long))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u1.npy", typeof(byte))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u2_be.npy", typeof(ushort))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u2_le.npy", typeof(ushort))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u4_be.npy", typeof(uint))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u4_le.npy", typeof(uint))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u8_be.npy", typeof(ulong))]
    [TestCase(@"Assets/Tests/Editor/Data/arange_u8_le.npy", typeof(ulong))]
    public static void ReadArray_Arange(string path, Type type)
    {
        // typeof(TestNpyReader)
        //     .GetMethod(nameof(ReadArray_ArangeGeneric))
        //     .MakeGenericMethod(type)
        //     .Invoke(null, new object[] { path });

        using Stream stream = File.OpenRead(path);
        BinaryReader reader = new(stream);
        NpyReader npyReader = new(reader);
        Array arr = npyReader.ReadArray();

        Assert.That(arr.Length, Is.GreaterThan(0));
        for (int i = 0; i < arr.Length; i++)
        {
            Assert.That(arr.GetValue(i), Is.EqualTo(i));
        }
    }

    [TestCase(@"Assets/Tests/Editor/Data/arange_f2_be.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/arange_f2_le.npy")]
    public static void ReadArray_ArangeOfHalf(string path)
    {
        using Stream stream = File.OpenRead(path);
        BinaryReader reader = new(stream);
        NpyReader npyReader = new(reader);
        Array arr = npyReader.ReadArray();

        Assert.That(arr.Length, Is.GreaterThan(0));
        for (int i = 0; i < arr.Length; i++)
        {
            Half expected = (Half)i;
            Assert.That(arr.GetValue(i), Is.EqualTo(expected));
        }
    }

    [TestCase(@"Assets/Tests/Editor/Data/hello_S5.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/hello_U5_be.npy")]
    [TestCase(@"Assets/Tests/Editor/Data/hello_U5_le.npy")]
    public static void ReadArray_HelloWorld(string path)
    {
        using Stream stream = File.OpenRead(path);
        BinaryReader reader = new(stream);
        NpyReader npyReader = new(reader);
        string[] arr = npyReader.ReadArray<string>();

        Assert.That(arr[0], Is.EqualTo("Hello"));
        Assert.That(arr[1], Is.EqualTo("World"));
    }

    // [TestCase(@"Assets/Tests/Editor/Data/scalar_S1.npy", 'X')]
    [TestCase(@"Assets/Tests/Editor/Data/scalar_b1.npy", false)]
    [TestCase(@"Assets/Tests/Editor/Data/scalar_i4_be.npy", 42)]
    [TestCase(@"Assets/Tests/Editor/Data/scalar_i4_le.npy", 42)]
    public static void ReadArray_Scalar<T>(string path, T expected)
    {
        using Stream stream = File.OpenRead(path);
        BinaryReader reader = new(stream);
        NpyReader npyReader = new(reader);
        Array arr = npyReader.ReadArray();

        Assert.That(arr.GetValue(0), Is.EqualTo(expected));
    }
}

// }