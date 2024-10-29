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

        // The following works during Play Mode, but not during Edit Mode:
        // [UnityEditor.Callbacks.DidReloadScripts]
        // static void OnDidReloadScripts()
        // {
        //     PlenOctreeRenderer[] renderers =
        //         FindObjectsByType<PlenOctreeRenderer>(FindObjectsSortMode.None);
        //     foreach (var renderer in renderers) {
        //         renderer.Init();
        //     }
        // }

        void Awake()
        {
            MaybeInitialize();
        }

        void Update()
        {
            // It might be neccessary to re-initialize this component when running in Edit Mode
            // in case of a scene or script reload.
            if (!Application.IsPlaying(gameObject)) {
                MaybeInitialize();
            }
            
            // The following doesn't work for some reason:
            // if (_nodeChildrenBuffer == null || !_nodeChildrenBuffer.IsValid()) {
            //     Debug.Log("Buffers missing/invalid; re-initializing");
            //     Init();
            // }
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

            // FIXME: Probably leaks when in editor mode!
            _material = null;
        }

        void MaybeInitialize()
        {
            if (_nodeChildrenBuffer == null) {
                InitializeNodeChildrenBuffer();
            }

            if (_nodeDataBuffer == null) {
                InitializeNodeDataBuffer();
            }

            // When running in Edit Mode, some events may cause the material to
            // lose its current properties, in which case it has to be re-initialized.
            if (_material == null || !_material.HasProperty("_SVOWidth")) {
                InitializeMaterial();
            }
        }

        void InitializeNodeChildrenBuffer()
        {   
            List<int> nodeChildren = _plenoctree.array.NodeChildrenBuffer.ToList();
            _nodeChildrenBuffer = new ComputeBuffer(nodeChildren.Count, sizeof(int));
            _nodeChildrenBuffer.SetData(nodeChildren);
        }

        void InitializeNodeDataBuffer()
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
            _nodeDataBuffer.SetData(nodeDataRaw);
        }

        void InitializeMaterial()
        {
            if (_plenoctree == null || _nodeChildrenBuffer == null || _nodeDataBuffer == null) {
                return;
            }

            if (_material == null) {
                // TODO: Double-check that doing this makes sense
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
