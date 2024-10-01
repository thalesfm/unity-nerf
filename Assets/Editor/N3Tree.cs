using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using NumSharp;
using NumSharp.Generic;
using UnityEngine;
using UnityNeRF.Editor.IO;

namespace UnityNeRF
{
    public class N3Tree
    {
        public int N;
        public DataFormat data_format;
        public int data_dim;
        public NDArray<float> data;
        public NDArray<int> child;
        public NDArray<int> parent_depth;
        public Vector3 invradius;
        public Vector3 offset;
        public int depth_limit;
        // private ... extra_data;

        private N3Tree()
        {
        }

        public static N3Tree Load(string path)
        {
            Stream stream = File.OpenRead(path);
            return Load(stream);
        }

        public static N3Tree Load(Stream stream)
        {
            var tree = new N3Tree();
            using var npz = new NpzFile(stream);

            tree.data_dim = (int)npz.ReadInt64("data_dim.npy");
            // Debug.Log($"data_dim = {tree.data_dim}");

            tree.child = (NDArray<int>)npz.ReadArray<int>("child.npy", out int[] shape);
            tree.child = tree.child.reshape(shape);
            // Debug.Log($"child = {tree.child}");

            tree.N = tree.child.shape[^1];
            // Assert N == 2

            if (npz.ContainsEntry("invradius3.npy"))
            {
                float[] invradius = npz.ReadArray<float>("invradius3.npy");
                tree.invradius = new Vector3(invradius[0], invradius[1], invradius[2]);
            }
            else
            {
                float invradius = npz.ReadSingle("invradius.npy");
                tree.invradius = new Vector3(invradius, invradius, invradius);
            }
            // Debug.Log($"invradius = {tree.invradius}");
            
            float[] offset = npz.ReadArray<float>("offset.npy");
            tree.offset = new Vector3(offset[0], offset[1], offset[2]);
            // Debug.Log($"offset = {tree.offset}");

            tree.depth_limit = (int)npz.ReadInt64("depth_limit.npy");
            // Debug.Log($"depth_limit = {tree.depth_limit}");

            Half[] data = npz.ReadArray<Half>("data.npy", out shape);
            tree.data = (NDArray<float>)data.Select(value => (float)value).ToArray();
            tree.data = tree.data.reshape(shape);
            // Debug.Log($"data = {tree.data}");

            // if (npz.ContainsEntry("data_format.npy"))
            tree.data_format = DataFormat.Parse(npz.ReadString("data_format.npy"));
            // Debug.Log($"data_format = {tree.data_format}");
            
            if (npz.ContainsEntry("extra_data.npy"))
                throw new NotSupportedException("Extra data found!");
            
            return tree;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NDArray this[float x, float y, float z] => Sample(x, y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NDArray Sample(float x, float y, float z) => Sample(new Vector3(x, y, z));

        public NDArray Sample(Vector3 coord)
        {
            coord = coord.Clamp(0.0f, 1.0f - 1e-10f);
            int ptr = 0;

            for (int i = 0; i <= depth_limit; ++i)
            {
                Vector3Int idx = (2.0f * coord).FloorToInt();
                int skip = child[ptr, idx.x, idx.y, idx.z];
                if (skip == 0)
                {
                    NDArray slice = ((NDArray) data)[ptr, idx.x, idx.y, idx.z, Slice.All];
                    return slice;
                }
                coord = 2.0f * coord - idx;
                ptr += skip;
            }

            throw new Exception();
        }
    }
} // namespace UnityNeRF