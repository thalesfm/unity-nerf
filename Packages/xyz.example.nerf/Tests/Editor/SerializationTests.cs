using System;
using System.IO;
using NUnit.Framework;

namespace UnityNeRF.Editor.Tests
{
    public class SerializationTests
    {
        public static SparseArray3D<float[]> Generate()
        {
            var array = new SparseArray3D<float[]>(100, 100, 100);

            for (int x = 0; x < 100; ++x)
            for (int y = 0; y < 100; ++y)
            for (int z = 0; z < 100; ++z)
            {
                array[x, y, z] = new float[] { x, y, z };
            }

            return array;
        }

        [Test]
        public static void SaveLoad_RoundTrip()
        {
            SparseArray3D<float[]> expected = Generate();
            
            using var stream = new MemoryStream();
            expected.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            SparseArray3D<float[]> actual = SparseArray3D.Load<float[]>(stream);

            Assert.That(actual.Width, Is.EqualTo(expected.Width));
            Assert.That(actual.Height, Is.EqualTo(expected.Height));
            Assert.That(actual.Depth, Is.EqualTo(expected.Depth));

            for (int x = 0; x < expected.Width; ++x)
            for (int y = 0; y < expected.Height; ++y)
            for (int z = 0; z < expected.Depth; ++z)
            {
                Assert.That(actual[x, y, z], Is.EqualTo(expected[x, y, z]));
            }
        }
    }
}
