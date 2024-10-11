using System;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;
using UnityEngine.Assertions;
using UnityNeRF.Editor.PlenOctree;

namespace UnityNeRF.Editor
{
    public static class Convert
    {
        public static SparseArray3D ToSparseArray3D(N3Tree tree)
        {
            return ToSparseArray3D(tree, int.MaxValue);
        }

        public static SparseArray3D ToSparseArray3D(N3Tree tree, int maxLevel)
        {
            if (tree.N != 2)
                throw new NotSupportedException();
            
            if (maxLevel > tree.depth_limit + 1)
                maxLevel = tree.depth_limit + 1;
            
            // int width, height, depth;
            // width = height = depth = 1 << maxLevel;
            // int data_dim = tree.data.shape[^1];
            
            var array = new SparseArray3D<float[]>(1 << maxLevel, 1 << maxLevel, 1 << maxLevel);
            
            foreach ((var index, var data) in Enumerate(tree))
            {
                array[index.Item1, index.Item2, index.Item3] = data;
            }

            return array;
        }

        public static SparseArray3D<T> ToSparseArray3D<T>(N3Tree tree)
        {
            return (SparseArray3D<T>) ToSparseArray3D(tree);
        }

        public static SparseArray3D<T> ToSparseArray3D<T>(N3Tree tree, int maxLevel)
        {
            return (SparseArray3D<T>) ToSparseArray3D(tree, maxLevel);
        }

        // Enumerate (corner, data) for every leaf node
        private static IEnumerable<KeyValuePair<(int, int, int), float[]>> Enumerate(N3Tree tree)
        {
            return Enumerate(tree, 0, (0, 0, 0));
        }

        private static IEnumerable<KeyValuePair<(int, int, int), float[]>> Enumerate(N3Tree tree, int node, (int, int, int) index)
        {
            for (int x = 0; x < tree.N; ++x)
            for (int y = 0; y < tree.N; ++y)
            for (int z = 0; z < tree.N; ++z)
            {
                (int, int, int) childIndex;
                childIndex.Item1 = tree.N * index.Item1 + x;
                childIndex.Item2 = tree.N * index.Item2 + y;
                childIndex.Item3 = tree.N * index.Item3 + z;

                int skip = tree.child.GetInt32(node, x, y, z);
                if (skip == 0) // Leaf node
                {
                    float[] data = new float[tree.data_dim];
                    tree.data[node, x, y, z, Slice.All].GetData().CopyTo(data.AsSpan());
                    yield return KeyValuePair.Create(childIndex, data);
                }
                else
                {
                    foreach (var pair in Enumerate(tree, node + skip, childIndex))
                        yield return pair;
                }
            }
        }

        // private static void CopyNodeRecursive(N3Tree source, SparseArray3D<SH16Voxel> dest, int index = 0, int level = 0)
        // {
        //     if (level >= dest.MaxLevel)
        //         return;

        //     for (int x = 0; x < 2; ++x)
        //     for (int y = 0; y < 2; ++y)
        //     for (int z = 0; z < 2; ++z)
        //     {
        //         int skip = source.child.GetInt32(index, x, y, z);
        //         int childIndex = (skip != 0) ? index + skip : dest.AddNode();

        //         float[] data = new float[source.data_dim];
        //         source.data[index, x, y, z, Slice.All].GetData().CopyTo(data.AsSpan());

        //         dest._nodeChildren[8*index + 4*z + 2*y + x] = childIndex;
        //         dest._nodeData[childIndex] = new SH16Voxel();

        //         if (skip != 0)
        //             CopyNodeRecursive(source, dest, childIndex, level + 1);
        //     }
        // }
    }
}
