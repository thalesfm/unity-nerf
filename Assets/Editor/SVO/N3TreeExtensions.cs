using System;
// using UnityNeRF;

namespace UnityNeRF
{
    public static class N3TreeExtensions
    {
        public static SparseVoxelOctree<float[]> ToSparseVoxelOctree(this N3Tree tree)
        {
            int width = (int) Math.Pow(tree.N, tree.depth_limit + 1);
            var octree = new SparseVoxelOctree<float[]>(width, width, width);
            Copy(tree, 0, octree, 0, 0, 0, 0);
            return octree;
        }

        // WARNING: Assumes source.N == 2
        private static void Copy(N3Tree source, int ptr, SparseVoxelOctree<float[]> dest, int destX, int destY, int destZ, int level)
        {
            for (int z = 0; z < 2; ++z)
            {
                for (int y = 0; y < 2; ++y)
                {
                    for (int x = 0; x < 2; ++x)
                    {
                        int skip = source.child[ptr, x, y, z];

                        if (skip == 0)
                        {
                            float[] coeffs = new float[source.data_dim];

                            for (int i = 0; i < source.data_dim; ++i)
                                coeffs[i] = source.data[ptr, x, y, z, i];

                            dest[destX, destY, destZ] = coeffs;
                        }
                        else
                        {
                            Copy(source, ptr + skip, dest, 2 * destX + x, 2 * destY + y, 2 * destZ + z, level + 1);
                        }
                    }
                }
            }
        }

        public static int FindMaxLevel(this N3Tree tree, int ptr = 0)
        {
            int maxLevel = 0;

            for (int x = 0; x < tree.N; ++x)
            {
                for (int y = 0; y < tree.N; ++y)
                {
                    for (int z = 0; z < tree.N; ++z)
                    {
                        var skip = tree.child[ptr, x, y, z];

                        if (skip != 0)
                        {
                            int level = 1 + tree.FindMaxLevel(ptr + skip);
                            maxLevel = Math.Max(level, maxLevel);
                        }
                    }
                }
            }

            return maxLevel;
        }
    }
}
