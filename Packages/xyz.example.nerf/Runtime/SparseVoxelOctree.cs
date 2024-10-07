using System;
using System.Collections.Generic;
using Google.Protobuf;
using System.IO;
using System.Runtime.InteropServices;
// using System.Linq;

namespace UnityNeRF
{
    public class SparseVoxelOctree<T> // : IEquatable
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        public readonly int MaxLevel;

        public List<int> _nodeChildren; // FIXME: Ideally should not be public
        public List<T> _nodeData;       // FIXME: Ideally should not be public

        public SparseVoxelOctree(int maxLevel)
        {
            Width = Height = Depth = (int) Math.Pow(2.0, maxLevel);
            MaxLevel = maxLevel;

            _nodeChildren = new List<int>();
            _nodeData = new List<T>();

            Clear();
        }

        public SparseVoxelOctree(int width, int height, int depth)
        {
            Width  = width;
            Height = height;
            Depth  = depth;

            MaxLevel = 0;
            MaxLevel = Math.Max((int) Math.Ceiling(Math.Log(width, 2.0)), MaxLevel);
            MaxLevel = Math.Max((int) Math.Ceiling(Math.Log(height, 2.0)), MaxLevel);
            MaxLevel = Math.Max((int) Math.Ceiling(Math.Log(depth, 2.0)), MaxLevel);

            _nodeChildren = new List<int>();
            _nodeData = new List<T>();

            Clear();
        }

        public static SparseVoxelOctree<float[]> Load(string path)
        {
            using var stream = File.OpenRead(path);
            return Load(stream);
        }

        // FIXME: Expects octree containining exactly 49 floats per voxel
        // (Corresponding to the SH16 format for an N3Tree)
        public static SparseVoxelOctree<float[]> Load(Stream stream)
        {
            var parser = Protobuf.SparseVoxelOctree.Parser;
            var message = parser.ParseFrom(stream);
            
            var svo = new SparseVoxelOctree<float[]>(message.Width, message.Height, message.Depth);
            svo._nodeChildren.Clear();
            svo._nodeChildren.AddRange(message.NodeChildren);
            svo._nodeData.Clear();
            
            int count = svo._nodeChildren.Count / 8;

            if (message.NodeData.Length != sizeof(float) * 49 * count) {
                stream.Close();
                throw new Exception("Expected exactly 49 floats");
            }

            var nodeData = message.NodeData.ToByteArray();
            for (int i = 0; i < count; ++i) {
                var coeffs = new float[49];
                for (int j = 0; j < 49; ++j) {
                    int offset = sizeof(float) * (49 * i + j);
                    float value = BitConverter.ToSingle(nodeData, offset);
                    coeffs[j] = value;
                }
                svo._nodeData.Add(coeffs);
            }

            stream.Close();
            return svo;
        }

        public static void Save(SparseVoxelOctree<float[]> octree, string path)
        {
            using var stream = File.OpenWrite(path);
            Save(octree, stream);
        }

        // FIXME: Expects octree containining exactly 49 floats per voxel
        // (Corresponding to the SH16 format for an N3Tree)
        public static void Save(SparseVoxelOctree<float[]> octree, Stream stream)
        {
            float[] nodeData = new float[octree._nodeData.Count * 49];

            for (int i = 0; i < octree._nodeData.Count; ++i)
            {
                if (octree._nodeData[i] == null)
                    continue;

                if (octree._nodeData[i].Length != 49)
                    throw new Exception("Expected exactly 49 floats");
                
                octree._nodeData[i].CopyTo(nodeData, 49 * i);

                // for (int k = 0; k < octree._nodeData[i].Length; ++k)
                //     nodeData[49*i + k] = octree._nodeData[i][k];
                // for (int k = octree._nodeData[i].Length; k < 49; ++k)
                //     nodeData[49*i + k] = 0.0f;
            }

            var message = new Protobuf.SparseVoxelOctree();
            message.TypeUrl = "type.googleapis.com/google.protobuf.FloatValue";
            message.Width = octree.Width;
            message.Height = octree.Height;
            message.Depth = octree.Depth;
            message.NodeChildren.AddRange(octree._nodeChildren);

            // message.NodeData = ByteString.CopyFrom(MemoryMarshal.Cast<float, byte>(nodeData.AsSpan()));
            byte[] bytes = new byte[sizeof(float) * nodeData.Length];
            Buffer.BlockCopy(nodeData, 0, bytes, 0, bytes.Length);
            message.NodeData = ByteString.CopyFrom(bytes);

            message.WriteTo(stream);
        }

        public T this[int x, int y, int z, int level = 0]
        {
            get {
                int nodeIndex = GetNodeIndex(x, y, z, level);
                if (nodeIndex == -1) {
                    return default;
                }
                return _nodeData[nodeIndex];
            }

            set {
                int nodeIndex = GetNodeIndex(x, y, z, level, true);
                _nodeData[nodeIndex] = value;
            }
        }

        public void Clear()
        {
            _nodeChildren.Clear();
            _nodeData.Clear();

            AddNode();
        }

        public void Remove(int x, int y, int z)
        {
            // TODO
        }

        // HACK
        public List<int> GetNodeChildren()
        {
            return _nodeChildren;
        }

        // HACK
        public List<T> GetNodeData()
        {
            return _nodeData;
        }

        // public T Sample(float x, float y, float z)
        // {
        //     int i = (int)(x * Width;
        //     int j = y * Height;
        //     int k = z * Depth;
        // }

        // FIXME: Ideally should not be public
        public int AddNode()
        {
            int nodeIndex = _nodeData.Count;

            _nodeChildren.AddRange(new int[] { -1, -1, -1, -1, -1, -1, -1, -1 });
            _nodeData.Add(default);

            return nodeIndex;
        }

        private bool IndexWithinRange(int x, int y, int z)
        {
            return 0 <= x && x < Width  &&
                0 <= y && y < Height &&
                0 <= z && z < Depth;
        }

        private int GetNodeIndex(int x, int y, int z, int level = 0, bool refine = false)
        {
            if (level > MaxLevel) {
                throw new IndexOutOfRangeException();
            }

            if (!IndexWithinRange(x, y, z)) {
                throw new IndexOutOfRangeException();
            }

            int nodeIndex = 0;

            for (int k = MaxLevel - 1; k >= level; --k)
            {   
                bool qx = (x & (1 << k)) != 0;
                bool qy = (y & (1 << k)) != 0;
                bool qz = (z & (1 << k)) != 0;

                int octant = 0;
                octant += qx ? 1 : 0;
                octant += qy ? 2 : 0;
                octant += qz ? 4 : 0;

                int childIndex = _nodeChildren[8 * nodeIndex + octant];
                if (childIndex == -1) {
                    if (!refine) {
                        return -1;
                    }
                    
                    childIndex = AddNode();
                    _nodeChildren[8 * nodeIndex + octant] = childIndex;
                }

                nodeIndex = childIndex;
            }

            return nodeIndex;
        }
    }
} // namespace UnityNeRF