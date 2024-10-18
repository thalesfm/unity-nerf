using System;
using System.Collections.Generic;
using UnityEngine;
using NumSharp;

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
            if (maxLevel > tree.depth_limit + 1)
                maxLevel = tree.depth_limit + 1;
            
            var array = new SparseArray3D<float[]>(1 << maxLevel, 1 << maxLevel, 1 << maxLevel);

            foreach ((Vector3Int index, float[] data) in tree.VoxelList)
            {
                array[index.x, index.y, index.z] = data;
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
    }
}
