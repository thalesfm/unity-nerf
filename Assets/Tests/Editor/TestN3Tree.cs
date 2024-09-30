using System;
using NUnit.Framework;
using NumSharp;
using UnityNeRF.Editor.IO;

namespace UnityNeRF.Editor.Tests
{
    public class TestN3Tree
    {
        [TestCase(@"Assets/Resources/oct_lego.npz")]
        public static void Load_NoThrow(string path)
        {
            N3Tree tree = N3Tree.Load(path);
            // UnityEngine.Debug.Log($"tree.depth_limit = {tree.depth_limit}");
            // UnityEngine.Debug.Log($"tree.FinMaxLevel() = {tree.FindMaxLevel()}");
        }

        [TestCase(@"Assets/Resources/oct_lego.npz")]
        public static void ToSparseVoxelOctree_NoThrow(string path)
        {
            UnityEngine.Debug.Log($"Loading plenoctree...");
            N3Tree tree = N3Tree.Load(path);
            UnityEngine.Debug.Log($"Converting to octree...");
            SparseVoxelOctree<float[]> octree = tree.ToSparseVoxelOctree();
        }
    }
} // namespace UnityNeRF.Editor.Tests