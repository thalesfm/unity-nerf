using System;
using System.Collections.Generic;
using NumSharp;
// using UnityNeRF;

namespace UnityNeRF.Editor
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
            
            if (maxLevel > tree.depth_limit + 1)
                maxLevel = tree.depth_limit + 1;
            
            var octree = new SparseVoxelOctree<float[]>(maxLevel);
            int nodeCount = tree.data.shape[0];
            
            // Initialize nodes
            for (int i = 1; i < nodeCount; ++i)
                octree.AddNode();
            
            CopyNodeRecursive(tree, octree);

            return octree;
        }

        private static void CopyNodeRecursive(N3Tree source, SparseVoxelOctree<float[]> dest, int index = 0, int level = 0)
        {
            if (level >= dest.MaxLevel)
                return;

            for (int x = 0; x < 2; ++x)
            for (int y = 0; y < 2; ++y)
            for (int z = 0; z < 2; ++z)
            {
                int skip = source.child.GetInt32(index, x, y, z);
                int childIndex = (skip != 0) ? index + skip : dest.AddNode();

                float[] data = new float[source.data_dim];
                source.data[index, x, y, z, Slice.All].GetData().CopyTo(data.AsSpan());

                dest._nodeChildren[8*index + 4*z + 2*y + x] = childIndex;
                dest._nodeData[childIndex] = data;

                if (skip != 0)
                    CopyNodeRecursive(source, dest, childIndex, level + 1);
            }
        }
    }
}
