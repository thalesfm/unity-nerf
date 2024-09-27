using System;
using System.IO;
using System.Linq;
using NumSharp;
using NumSharp.Generic;
using UnityEngine;
using UnityNeRF.Editor.IO;

namespace UnityNeRF
{
    public class N3Tree
    {
        private int N;
        private DataFormat data_format;
        private int data_dim;
        private NDArray<float> data;
        private NDArray<int> child;
        private NDArray<int> parent_depth;
        private Vector3 invradius;
        private Vector3 offset;
        private int depth_limit;
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

            tree.child = (NDArray<int>)npz.ReadArray<int>("child.npy", out int[] shape);
            tree.child.reshape(shape);

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
            
            float[] offset = npz.ReadArray<float>("offset.npy");
            tree.offset = new Vector3(offset[0], offset[1], offset[2]);

            tree.depth_limit = (int)npz.ReadInt64("depth_limit.npy");

            Half[] data = npz.ReadArray<Half>("data.npy", out shape);
            tree.data = (NDArray<float>)data.Select(value => (float)value).ToArray();
            tree.data.reshape(shape);

            if (npz.ContainsEntry("data_format.npy"))
                tree.data_format = DataFormat.Parse(npz.ReadString("data_format.npy"));
            
            if (npz.ContainsEntry("extra_data.npy"))
                throw new NotSupportedException("Extra data found!");
            
            return tree;
        }
    }
} // namespace UnityNeRF