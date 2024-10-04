using System.Collections.Generic;
using UnityEngine;

namespace UnityNeRF
{

    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public class SparseVoxelOctreeRenderer : MonoBehaviour
    {
        public string _fileName;

        private Material _material;
        private UnityNeRF.SparseVoxelOctree<float[]> _voxelOctree;
        private ComputeBuffer _nodeChildrenBuffer;
        private ComputeBuffer _nodeDataBuffer;
    
        void Awake()
        {
            if (_fileName == null) {
                Debug.LogWarning("Voxel grid file name is missing!");
            }

            Init();
        }

        void Update()
        {
            // It might be neccessary to re-initialize this component when running in Edit Mode
            // in case of a scene or script reload.
            if (!Application.IsPlaying(gameObject)) {
                Init();
            }
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
            if (_voxelOctree == null) {
                InitVoxelOctree();
            }

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

        void InitVoxelOctree()
        {
            if (_fileName == null) {
                return;
            }

            _voxelOctree = SparseVoxelOctree<float[]>.Load(_fileName);
        }

        void InitNodeChildrenBuffer()
        {
            if (_voxelOctree == null) {
                return;
            }
            
            List<int> nodeChildren = _voxelOctree.GetNodeChildren();
            _nodeChildrenBuffer = new ComputeBuffer(nodeChildren.Count, sizeof(int));
            _nodeChildrenBuffer.SetData<int>(nodeChildren);
        }

        void InitNodeDataBuffer()
        {
            if (_voxelOctree == null) {
                return;
            }

            List<float[]> nodeData = _voxelOctree.GetNodeData();
            List<float> nodeDataRaw = new List<float>(49 * nodeData.Count);

            for (int i = 0; i < nodeData.Count; ++i) {
                for (int j = 0; j < 49; ++j) {
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
            if (_voxelOctree == null || _nodeChildrenBuffer == null || _nodeDataBuffer == null) {
                return;
            }

            if (_material == null) {
                _material = GetComponent<MeshRenderer>().sharedMaterial;
            }

            _material.SetInt("_SVOWidth", _voxelOctree.Width);
            _material.SetInt("_SVOHeight", _voxelOctree.Height);
            _material.SetInt("_SVODepth", _voxelOctree.Depth);
            _material.SetInt("_SVOMaxLevel", _voxelOctree.MaxLevel);
            _material.SetBuffer("_SVONodeChildren", _nodeChildrenBuffer);
            _material.SetBuffer("_SVONodeData", _nodeDataBuffer);
        }
    }
} // namespace UnityNeRF