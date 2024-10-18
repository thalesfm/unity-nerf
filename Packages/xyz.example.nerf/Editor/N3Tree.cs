using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using NumSharp;
using NumSharp.Generic;
using UnityNeRF.Editor.IO;
using System.Collections;

namespace UnityNeRF.Editor
{
    public class N3Tree // : IEnumerable<KeyValuePair<Vector3Int, float[]>>
    {
        public int N;
        public int data_dim { get; private set; }
        public int depth_limit;
        public DataFormat data_format { get; private set; }
        // private ... extra_data;

        private NDArray<float> data;
        private NDArray<int> child;
        private NDArray<int> parent_depth;
        private Vector3 invradius;
        private Vector3 offset;
        private int n_internal;

        private N3Tree()
        { }

        public Vector3 Radius
        {
            get => new Vector3(
                0.5f / invradius.x,
                0.5f / invradius.y,
                0.5f / invradius.z
            );
            set => invradius = new Vector3(
                0.5f / value.x,
                0.5f / value.y,
                0.5f / value.z
            );
        }

        public Vector3 Center
        {
            get => new Vector3(
                (offset.x + 0.5f) * invradius.x,
                (offset.y + 0.5f) * invradius.y,
                (offset.z + 0.5f) * invradius.z
            );
            set => offset = new Vector3(
                0.5f - value.x / invradius.x,
                0.5f - value.y / invradius.y,
                0.5f - value.z / invradius.z
            );
        }

        public IEnumerable<KeyValuePair<Vector3Int, float[]>> VoxelList
        {
            get { return GetVoxelList(0, -1, new Vector3Int(0, 0, 0)); }
        }

        // public IEnumerator<KeyValuePair<Vector3Int, float[]>> GetEnumerator() => VoxelList.GetEnumerator();

        // IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // public void Save(string path) => throw new NotImplementedException();

        // public void Save(Stream stream) => throw new NotImplementedException();

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

            tree.n_internal = (int) z.ReadInt64("n_internal.npy");

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

        private IEnumerable<KeyValuePair<Vector3Int, float[]>> GetVoxelList(int nid, int depth, Vector3Int acc)
        {
            for (int x = 0; x < N; ++x)
            for (int y = 0; y < N; ++y)
            for (int z = 0; z < N; ++z)
            {
                var index = N*acc + new Vector3Int(x, y, z);

                if (depth == depth_limit - 1)
                {
                    float[] datum = ((NDArray) data)[nid, x, y, z].ToArray<float>();
                    yield return KeyValuePair.Create(index, datum);
                }

                int skip = child.GetInt32(nid, x, y, z);
                if (skip == 0) continue;

                foreach (var voxel in GetVoxelList(nid + skip, depth + 1, index))
                {
                    yield return voxel;
                }
            }
        }
    }
}
