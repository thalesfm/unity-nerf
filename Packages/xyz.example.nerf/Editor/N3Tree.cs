using System;
using System.IO;
using System.Linq;
// using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using NumSharp;
using NumSharp.Generic;
using UnityEngine;
using UnityNeRF.Editor.IO;

namespace UnityNeRF.Editor
{
    public class N3Tree // : IEnumerable<N3TreeNode>
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

        private int _n_internal;

        // public IEnumerable<N3TreeNode> Frontier => throw new NotImplementedException();

        private N3Tree()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NDArray forward(float x, float y, float z) => forward(new Vector3(x, y, z));

        public NDArray forward(Vector3 indices, bool world = true)
        {   
            if (world)
                indices = world2tree(indices);
            
            indices = indices.Clamp(0.0f, 1.0f - 1e-10f);

            int node_id = 0;
            Vector3 ind = indices;

            for (int i = 0; i <= depth_limit; ++i)
            {
                ind *= N;
                Vector3Int ind_floor = ind.FloorToInt();
                ind_floor.Clamp(0, N - 1);
                ind -= ind_floor;

                int[] sel = new int[] { node_id, ind_floor.x, ind_floor.y, ind_floor.z };

                int delta = child[sel];

                if (delta == 0)
                    return ((NDArray) data)[node_id, ind_floor.x, ind_floor.y, ind_floor.z, Slice.All];

                node_id += delta;
            }

            throw new Exception();
        }

        public static N3Tree Load(string path)
        {
            Stream stream = File.OpenRead(path);
            return Load(stream);
        }

        public static N3Tree Load(Stream stream)
        {
            var tree = new N3Tree();
            using var z = new NpzFile(stream);
            tree.data_dim = (int) z.ReadInt64("data_dim.npy");
            tree.child = (NDArray<int>) z.ReadArray<int>("child.npy", out int[] shape);
            tree.child = tree.child.reshape(shape);
            tree.N = tree.child.shape[^1];
            tree.parent_depth = (NDArray<int>) z.ReadArray<int>("parent_depth.npy", out shape);
            tree.parent_depth = tree.parent_depth.reshape(shape);
            // Debug.Log($"parent_depth.shape = {(NDArray<int>) tree.parent_depth.shape}");
            tree._n_internal = (int) z.ReadInt64("n_internal.npy");
            if (z.ContainsEntry("invradius3.npy")) {
                float[] invradius = z.ReadArray<float>("invradius3.npy");
                tree.invradius = new Vector3(invradius[0], invradius[1], invradius[2]);
            } else {
                float invradius = z.ReadSingle("invradius.npy");
                tree.invradius = new Vector3(invradius, invradius, invradius);
            }
            float[] offset = z.ReadArray<float>("offset.npy");
            tree.offset = new Vector3(offset[0], offset[1], offset[2]);
            tree.depth_limit = (int) z.ReadInt64("depth_limit.npy");
            // tree.geom_resize_fact = ...
            Half[] data = z.ReadArray<Half>("data.npy", out shape);
            tree.data = (NDArray<float>) data.Select(value => (float) value).ToArray();
            tree.data = tree.data.reshape(shape);
            // tree._n_free = ...
            if (z.ContainsEntry("data_format.npy"))
                tree.data_format = DataFormat.Parse(z.ReadString("data_format.npy"));
            if (z.ContainsEntry("extra_data.npy"))
                throw new NotSupportedException("Extra data found!");
            
            return tree;
        }

        public Vector3 world2tree(Vector3 indices)
        {
            float x = offset.x + indices.x * invradius.z;
            float y = offset.y + indices.y * invradius.y;
            float z = offset.z + indices.z * invradius.z;

            return new Vector3(x, y, z);
        }

        public Vector3 tree2world(Vector3 indices)
        {
            float x = (indices.x - offset.x) / invradius.x;
            float y = (indices.y - offset.y) / invradius.y;
            float z = (indices.z - offset.z) / invradius.z;

            return new Vector3(x, y, z);
        }

        // public IEnumerator<N3TreeNode> GetEnumerator()
        // {
        //     throw new NotImplmentedException();
        // }

        // IEnumerator IEnumerable.GetEnumerator()
        // {
        //     throw new NotImplementedException();
        // }
    }

    // public readonly struct N3TreeNode
    // {
    //     public readonly N3Tree Tree;
    //     public readonly int Key;

    //     // public NDArray Corners => throw new NotImplementedException();

    //     public NDArray CornersLocal => throw new NotImplementedException();

    //     public readonly NDArray<float> Data =>
    //         (NDArray<float>) ((NDArray) Tree.data)[Key, Slice.All];

    //     public readonly int Depth => Tree.parent_depth[Key, 1];

    //     public readonly bool IsInternal => Depth < Tree.depth_limit;

    //     internal N3TreeNode(N3Tree tree, int key, (float, float, float) index)
    //     {
    //         Tree = tree;
    //         Key = key;
    //     }
    // }
}
