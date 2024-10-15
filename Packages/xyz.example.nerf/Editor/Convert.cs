using System;
using System.Collections.Generic;
using NumSharp;
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
    }
}
