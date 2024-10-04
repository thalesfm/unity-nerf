using System.Collections.Generic;
using NUnit.Framework;
using NumSharp;
using UnityEngine;

namespace UnityNeRF.Editor.Tests
{
    public class ConversionTests
    {
        [TestCase(@"Packages/xyz.example.nerf/Tests/Editor/Data/svo_mgrid_16.zip")]
        public static void ToSparseVoxelOctree_Equal(string path, int samples = 20)
        {
            N3Tree tree = N3Tree.Load(path);
            
            // Assert(tree.data_format.format == RGB)
            // Assert(tree.data_dim == 3)

            SparseVoxelOctree<float[]> octree = Convert.ToSparseVoxelOctree(tree);
            System.Random random = new System.Random();

            for (int i = 0; i < octree.Width;  ++i)
            for (int j = 0; j < octree.Height; ++j)
            for (int k = 0; k < octree.Depth;  ++k)
            {
                float x = (i + 0.5f) / octree.Width;
                float y = (j + 0.5f) / octree.Height;
                float z = (k + 0.5f) / octree.Depth;

                float[] expected = tree.Sample(new Vector3(x, y, z)).ToArray<float>();
                float[] actual = octree[i, j, k];

                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }
}