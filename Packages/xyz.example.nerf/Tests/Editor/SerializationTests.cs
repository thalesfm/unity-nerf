using System;
using System.IO;
using NUnit.Framework;

namespace UnityNeRF.Editor.Tests
{
    public class SerializationTests
    {
        public static SparseVoxelOctree<float[]> Generate()
        {
            var octree = new SparseVoxelOctree<float[]>(100, 100, 100, 3);

            for (int x = 0; x < 100; ++x)
            for (int y = 0; y < 100; ++y)
            for (int z = 0; z < 100; ++z)
            {
                octree[x, y, z] = new float[] { x, y, z };
            }

            return octree;
        }

        [Test]
        public static void SaveLoad_RoundTrip()
        {
            SparseVoxelOctree<float[]> expected = Generate();
            
            using var stream = new MemoryStream();
            expected.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            SparseVoxelOctree<float[]> actual = SparseVoxelOctree.Load<float[]>(stream);

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
