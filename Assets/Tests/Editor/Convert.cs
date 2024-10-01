using NUnit.Framework;
using NumSharp;
using UnityEngine;

namespace UnityNeRF.Editor.Tests
{
    public class TestConvert
    {
        [TestCase(@"Assets/Tests/Editor/Data/svo_mgrid_16.npz")]
        public static void ToSparseVoxelOctree_Equal(string path, int samples = 20)
        {
            N3Tree tree = N3Tree.Load(path);
            SparseVoxelOctree<float[]> octree = Convert.ToSparseVoxelOctree(tree);
            System.Random random = new System.Random();

            for (int k = 0; k < samples; ++k)
            {
                float x = (float) random.NextDouble();
                float y = (float) random.NextDouble();
                float z = (float) random.NextDouble();
                NDArray expected = tree.Sample(new Vector3(x, y, z));
                Debug.Log($"tree.Sample({x}, {y}, {z}) = {expected}");
            }

            for (int k = 0; k < samples; ++k)
            {
                float x = (float) random.NextDouble();
                float y = (float) random.NextDouble();
                float z = (float) random.NextDouble();
                NDArray expected = tree.Sample(new Vector3(x, y, z));
                // UnityEngine.Debug.Log($"expected = {expected}");

                int xx = (int) (x * octree.Width  - 0.5);
                int yy = (int) (y * octree.Height - 0.5);
                int zz = (int) (z * octree.Depth  - 0.5);
                Debug.Log($"Testing node at ({xx}, {yy}, {zz}) (coord. ({x}, {y}, {z}))");
                float[] coeffs = octree[xx, yy, zz];
                // Debug.Log($"coeffs = {coeffs}");

                for (int i = 0; i < tree.data_dim; ++i)
                {
                    Assert.That(coeffs[i], Is.EqualTo(expected.GetSingle(i)));
                }

                // int nodeCount = tree.data.shape[0];
                // int index = random.Next(nodeCount);

                // for (int x = 0; x < 2; ++x)
                // {
                //     for (int y = 0; y < 2; ++y)
                //     {
                //         for (int z = 0; z < 2; ++z)
                //         {
                //             int skip = tree.child.GetInt32(index, x, y, z);
                //             for (int i = 0; i < tree.data_dim; ++i)
                //             {
                //                 float expected = tree.data.GetSingle(index, x, y, z, i);
                //                 float value = octree._node
                //             }
                //         }
                //     }
                // }
                
                Debug.Log($"Node at ({xx}, {yy}, {zz}) ok.");
            }
        }
    }
}