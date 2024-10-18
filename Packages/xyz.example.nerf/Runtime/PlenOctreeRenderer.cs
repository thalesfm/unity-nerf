using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityNeRF
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public class PlenOctreeRenderer : MonoBehaviour
    {
        public PlenOctree _plenoctree;

        private Material _material;
        private ComputeBuffer _nodeChildrenBuffer;
        private ComputeBuffer _nodeDataBuffer;

        void Update()
        {
            // It might be neccessary to re-initialize this component when running in Edit Mode
            // in case of a scene or script reload.
            if (!Application.IsPlaying(gameObject)) {
                Init();
            }
        }

        void Awake()
        {
            Init();
        }

        void OnDisable()
        {
            // Resources should be released when running in Edit Mode since this is the only callback
            // triggered on a scene reload.
            if (!Application.IsPlaying(gameObject)) {
                OnDestroy();
            }
        }

        void OnDestroy()
        {
            if (_nodeChildrenBuffer != null) {
                _nodeChildrenBuffer.Release();
                _nodeChildrenBuffer = null;
            }

            if (_nodeDataBuffer != null) {
                _nodeDataBuffer.Release();
                _nodeDataBuffer = null;
            }

            _material = null;
        }

        void Init()
        {
            if (_nodeChildrenBuffer == null) {
                InitNodeChildrenBuffer();
            }

            if (_nodeDataBuffer == null) {
                InitNodeDataBuffer();
            }

            // When running in Edit Mode, some events may cause the material to
            // lose its current properties, in which case it has to be re-initialized.
            if (_material == null || !_material.HasProperty("_Width")) {
                InitMaterial();
            }
        }

        void InitNodeChildrenBuffer()
        {   
            List<int> nodeChildren = _plenoctree.array.NodeChildrenBuffer.ToList();
            _nodeChildrenBuffer = new ComputeBuffer(nodeChildren.Count, sizeof(int));
            _nodeChildrenBuffer.SetData<int>(nodeChildren);
        }

        void InitNodeDataBuffer()
        {
            List<float[]> nodeData = _plenoctree.array.NodeDataBuffer.ToList();
            List<float> nodeDataRaw = new List<float>(_plenoctree.format.data_dim * nodeData.Count);

            for (int i = 0; i < nodeData.Count; ++i) {
                for (int j = 0; j < _plenoctree.format.data_dim; ++j) {
                    if (nodeData[i] != null) {
                        nodeDataRaw.Add(nodeData[i][j]);
                    } else {
                        nodeDataRaw.Add(0.0f);
                    }
                }
            }

            _nodeDataBuffer = new ComputeBuffer(nodeDataRaw.Count, sizeof(float));
            _nodeDataBuffer.SetData<float>(nodeDataRaw);
        }

        void InitMaterial()
        {
            if (_plenoctree == null || _nodeChildrenBuffer == null || _nodeDataBuffer == null) {
                return;
            }

            if (_material == null) {
                _material = GetComponent<MeshRenderer>().sharedMaterial;
            }

            _material.SetInt("_SVOWidth", _plenoctree.array.Width);
            _material.SetInt("_SVOHeight", _plenoctree.array.Height);
            _material.SetInt("_SVODepth", _plenoctree.array.Depth);
            _material.SetInt("_SVOBasisDim", (_plenoctree.format.data_dim - 1) / 3);
            _material.SetInt("_SVODataDim", _plenoctree.format.data_dim);
            _material.SetInt("_SVOMaxLevel", _plenoctree.array.MaxLevel);
            _material.SetBuffer("_SVONodeChildren", _nodeChildrenBuffer);
            _material.SetBuffer("_SVONodeData", _nodeDataBuffer);
        }
    }
} // namespace UnityNeRF
