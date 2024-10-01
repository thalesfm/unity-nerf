using System;
using System.Collections.Generic;
// using UnityNeRF;

namespace UnityNeRF
{
    public static partial class Convert
    {
        public static SparseVoxelOctree<float[]> ToSparseVoxelOctree(N3Tree tree)
        {
            return ToSparseVoxelOctree(tree, int.MaxValue);
        }

        public static SparseVoxelOctree<float[]> ToSparseVoxelOctree(N3Tree tree, int maxLevel)
        {
            if (tree.N != 2)
                throw new NotSupportedException();
            
            if (maxLevel > tree.depth_limit)
                maxLevel = tree.depth_limit;
            
            var octree = new SparseVoxelOctree<float[]>(maxLevel);
            int nodeCount = tree.data.shape[0];
            
            // Initialize nodes
            for (int i = 1; i < nodeCount; ++i)
                octree.AddNode();
            
            CopyNodeRecursive(tree, octree);
            // UnityEngine.Debug.Log($"count = {count}, total = {total}");

            // int total = 0;
            // for (int i = 0; i < count; ++i)
            //     if (octree._nodeData[i] == null)
            //         total += 1;
            // UnityEngine.Debug.Log($"Number of null entries: {total}");

            // for (int i = 0; i < 8; ++i)
            // {
            //     int value = octree._nodeChildren[i];
            //     UnityEngine.Debug.Log($"child[i] = {value}");
            // }

            return octree;
        }

        private static void CopyNodeRecursive(N3Tree source, SparseVoxelOctree<float[]> dest, int index = 0, int level = 0)
        {
            if (level >= dest.MaxLevel)
                return;

            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int z = 0; z < 2; ++z)
                    {
                        float[] coeffs = new float[source.data_dim];
                        for (int k = 0; k < source.data_dim; ++k)
                            coeffs[k] = source.data.GetSingle(index, x, y, z, k);

                        int skip = source.child.GetInt32(index, x, y, z);   
                        dest._nodeData[index + skip] = coeffs;

                        if (skip == 0)
                            continue;
                        
                        dest._nodeChildren[8*index + 4*z + 2*y + x] = index + skip;
                        CopyNodeRecursive(source, dest, index + skip, level + 1);
                    }
                }
            }
        }
    }
}
