using System;
using NUnit.Framework;
using NumSharp;
using UnityEngine;

namespace UnityNeRF.Editor.Tests
{
    public class TestN3Tree
    {
        [TestCase(@"Assets/Resources/oct_lego.npz", Ignore = "Slow")]
        public static void Load_NoThrow(string path)
        {
            N3Tree tree = N3Tree.Load(path);
        }

        [TestCase(@"Assets/Resources/oct_lego.npz", Ignore = "Slow")]
        public static void ToSparseVoxelOctree_NoThrow(string path)
        {
            UnityEngine.Debug.Log($"Loading plenoctree...");
            N3Tree tree = N3Tree.Load(path);
            UnityEngine.Debug.Log($"Converting to octree...");
            SparseVoxelOctree<float[]> octree = tree.ToSparseVoxelOctree();
        }

        [TestCase(@"Assets/Resources/oct_lego.npz")] // , Explicit = true, Ignore = "Slow")]
        public static void ToSparseVoxelOctree_Equal(string path, int samples = 20)
        {
            N3Tree tree = N3Tree.Load(path);
            SparseVoxelOctree<float[]> octree = tree.ToSparseVoxelOctree();
            System.Random random = new System.Random();

            for (int k = 0; k < samples; ++k)
            {
                float x = (float) random.NextDouble();
                float y = (float) random.NextDouble();
                float z = (float) random.NextDouble();
                NDArray expected = tree.Sample(new Vector3(x, y, z));

                int xx = (int) x * octree.Width;
                int yy = (int) y * octree.Height;
                int zz = (int) z * octree.Depth;
                float[] coeffs = octree[xx, yy, zz];

                for (int i = 0; i < tree.data_dim; ++i)
                    Assert.That(coeffs[i], Is.EqualTo(expected.GetSingle(i)));
            }
        }
    }
} // namespace UnityNeRF.Editor.Tests