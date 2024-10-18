using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace UnityNeRF
{
    [Serializable]
    public partial class SparseArray3D<T> : SparseArray3D, IEnumerable<KeyValuePair<Vector3Int, T>>
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int Depth;
        public readonly int MaxLevel;
        
        public List<int> _nodeChildren; // FIXME: Ideally should not be public
        public List<T> _nodeData;       // FIXME: Ideally should not be public

        public SparseArray3D(int width, int height, int depth)
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

        // public SparseArray3D(IEnumerable<KeyValuePair<Vector3Int, T>> collection) => throw new NotImplementedException();

        public T this[int x, int y, int z, int level = 0]
        {
            get
            {
                int nodeIndex = GetNodeIndex(x, y, z, level);
                if (nodeIndex == -1) {
                    return default;
                }
                return _nodeData[nodeIndex];
            }

            set
            {
                int nodeIndex = GetNodeIndex(x, y, z, level, true);
                _nodeData[nodeIndex] = value;
            }
        }

        public ReadOnlyCollection<int> NodeChildrenBuffer
        {
            get { return _nodeChildren.AsReadOnly(); }
        }

        public ReadOnlyCollection<T> NodeDataBuffer
        {
            get { return _nodeData.AsReadOnly(); }
        }

        // IEnumerable<T> interface implementation

        public IEnumerator<KeyValuePair<Vector3Int, T>> GetEnumerator()
        {
            return Enumerate(0, Vector3Int.zero, 0).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _nodeChildren.Clear();
            _nodeData.Clear();

            AddNode();
        }

        private int AddNode()
        {
            int nodeIndex = _nodeData.Count;

            _nodeChildren.AddRange(new int[] { -1, -1, -1, -1, -1, -1, -1, -1 });
            _nodeData.Add(default);

            return nodeIndex;
        }

        // private void Add(Vector3Int index, T value)

        private void AddRange(IEnumerable<KeyValuePair<Vector3Int, T>> collection)
        {
            foreach ((Vector3Int index, T value) in collection)
                this[index.x, index.y, index.z] = value;
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

        private IEnumerable<KeyValuePair<Vector3Int, T>> Enumerate(int index, Vector3Int acc, int level)
        {
            if (level == MaxLevel) // Leaf node
            {
                yield return KeyValuePair.Create(acc, _nodeData[index]);
                yield break;
            }

            for (int qx = 0; qx < 2; ++qx)
            for (int qy = 0; qy < 2; ++qy)
            for (int qz = 0; qz < 2; ++qz)
            {
                int childId = _nodeChildren[8*index + 4*qz + 2*qy + qx];
                if (childId == -1)
                    continue;
                
                int x = 2 * acc.x + qx;
                int y = 2 * acc.y + qy;
                int z = 2 * acc.z + qz;

                foreach (var entry in Enumerate(childId, new Vector3Int(x, y, z), level + 1))
                    yield return entry;
            }
        }
    }
} // namespace UnityNeRF
