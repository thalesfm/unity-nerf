using System;
using NUnit.Framework;

namespace UnityNeRF.Editor.Tests
{
    public class N3TreeConversionTest
    {
        [TestCase(@"Packages/xyz.example.nerf/Tests/Editor/Data/svo_mgrid_16.zip")]
        public static void Load_Mgrid(string path)
        {
            N3Tree tree = N3Tree.Load(path);
            SparseArray3D<float[]> array = Convert.ToSparseArray3D<float[]>(tree);

            for (int x = 0; x < array.Width;  ++x)
            for (int y = 0; y < array.Height; ++y)
            for (int z = 0; z < array.Depth;  ++z)
            {
                Assert.That(array[x, y, z][0], Is.EqualTo((float) x));
                Assert.That(array[x, y, z][1], Is.EqualTo((float) y));
                Assert.That(array[x, y, z][2], Is.EqualTo((float) z));
            }
        }
    }
}
