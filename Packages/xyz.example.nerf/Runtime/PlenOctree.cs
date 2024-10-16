using UnityEngine;
using System;
using System.Collections.Generic;

namespace UnityNeRF
{
    [PreferBinarySerialization]
    public class PlenOctree : ScriptableObject, ISerializationCallbackReceiver
    {
        public DataFormat format;

        [HideInInspector, NonSerialized]
        public SparseArray3D<float[]> array;

        [SerializeField]
        private int width;
        [SerializeField]
        private int height;
        [SerializeField]
        private int depth;
        [SerializeField]
        private List<int> nodeChildren;
        [SerializeField]
        private List<float> nodeData;

        public void OnBeforeSerialize()
        {
            if (array == null)
                return;
            
            width = array.Width;
            height = array.Height;
            depth = array.Depth;
            nodeChildren = GetNodeChildren();
            nodeData = GetNodeDataFlattened();
        }

        public void OnAfterDeserialize()
        {
            array = new SparseArray3D<float[]>(width, height, depth);
            array._nodeChildren = nodeChildren;
            array._nodeData = new List<float[]>();

            for (int start = 0; start < nodeData.Count; start += format.data_dim)
            {
                float[] datum = new float[format.data_dim];
                for (int k = 0; k < format.data_dim; ++k)
                    datum[k] = nodeData[start + k];
                array._nodeData.Add(datum);
            }
        }

        public List<int> GetNodeChildren()
        {
            return array.GetNodeChildren();
        }

        public List<float> GetNodeDataFlattened()
        {
            List<float[]> nodeData = array.GetNodeData();
            List<float> nodeDataFlattened = new List<float>(format.data_dim * nodeData.Count);

            for (int i = 0; i < nodeData.Count; ++i)
            {    
                if (nodeData[i] != null)
                {
                    nodeDataFlattened.AddRange(nodeData[i]);
                }
                else
                {
                    for (int k = 0; k < format.data_dim; ++k)
                        nodeDataFlattened.Add(0.0f);
                }
            }

            return nodeDataFlattened;
        }
    }
}
