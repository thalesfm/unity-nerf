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
        // Shader property IDs
        private static readonly int widthId = Shader.PropertyToID("_SVOWidth");
        private static readonly int heightId = Shader.PropertyToID("_SVOHeight");
        private static readonly int depthId = Shader.PropertyToID("_SVODepth");
        private static readonly int basisDimId = Shader.PropertyToID("_SVOBasisDim");
        private static readonly int dataDimId = Shader.PropertyToID("_SVODataDim");
        private static readonly int maxLevelId = Shader.PropertyToID("_SVOMaxLevel");
        private static readonly int nodeChildrenId = Shader.PropertyToID("_SVONodeChildren");
        private static readonly int nodeDataId = Shader.PropertyToID("_SVONodeData");

        private static MaterialPropertyBlock block;

        public PlenOctree _plenoctree;

        private ComputeBuffer _nodeChildrenBuffer;
        private ComputeBuffer _nodeDataBuffer;

        void Awake()
        {
            Init();
        }

        // void OnValidate()
        // {
        //     Init();
        // }

#if UNITY_EDITOR
        void OnEnable()
        {
            if (!Application.IsPlaying(gameObject)) {
                Init();
            }
        }

        void OnDisable()
        {
            // Resources should be released when running in Edit Mode since this is the
            // only callback triggered on a scene reload.
            if (!Application.IsPlaying(gameObject)) {
                Dispose();
            }
        }
#endif

        void OnDestroy()
        {
            Dispose();
        }

        void Init()
        {
            if (_nodeChildrenBuffer == null)
            {
                List<int> nodeChildren = _plenoctree.array.NodeChildrenBuffer.ToList();
                _nodeChildrenBuffer = new ComputeBuffer(nodeChildren.Count, sizeof(int));
                _nodeChildrenBuffer.SetData(nodeChildren);
            }

            if (_nodeDataBuffer == null)
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

            if (_plenoctree == null)
            {
                Debug.LogWarning("Failed to initialize material!");
                return;
            }

            block ??= new MaterialPropertyBlock();
            block.SetInt(widthId, _plenoctree.array.Width);
            block.SetInt(heightId, _plenoctree.array.Height);
            block.SetInt(depthId, _plenoctree.array.Depth);
            block.SetInt(basisDimId, (_plenoctree.format.data_dim - 1) / 3);
            block.SetInt(dataDimId, _plenoctree.format.data_dim);
            block.SetInt(maxLevelId, _plenoctree.array.MaxLevel);
            block.SetBuffer(nodeChildrenId, _nodeChildrenBuffer);
            block.SetBuffer(nodeDataId, _nodeDataBuffer);

            GetComponent<Renderer>().SetPropertyBlock(block);
        }

        void Dispose()
        {
            if (_nodeChildrenBuffer != null) {
                _nodeChildrenBuffer.Release();
                _nodeChildrenBuffer = null;
            }

            if (_nodeDataBuffer != null) {
                _nodeDataBuffer.Release();
                _nodeDataBuffer = null;
            }
        }
    }
} // namespace UnityNeRF
