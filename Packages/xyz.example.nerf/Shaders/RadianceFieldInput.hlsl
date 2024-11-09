#ifndef INPUT_INCLUDED
#define INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/SparseVoxelOctree.hlsl"

#define _Scale 0.5 // TODO: Hacky, remove
int _MaxSteps;
float _StepSize;

CBUFFER_START(UnityPerMaterial)
    float _Cutoff;
    float _MinTransmittance;
    int _SVOWidth;
    int _SVOHeight;
    int _SVODepth;
    int _SVOBasisDim;
    int _SVODataDim;
    int _SVOMaxLevel;
    StructuredBuffer<int> _SVONodeChildren;
    StructuredBuffer<float> _SVONodeData;
CBUFFER_END

SparseVoxelOctree GetSparseVoxelOctree()
{
    SparseVoxelOctree svo;
    svo.width = _SVOWidth;
    svo.height = _SVOHeight;
    svo.depth = _SVODepth;
    svo.basisDim = _SVOBasisDim;
    svo.dataDim = _SVODataDim;
    svo.maxLevel = _SVOMaxLevel;
    svo.nodeChildren = _SVONodeChildren;
    svo.nodeData = _SVONodeData;
    return svo;
}

float GetNodeDensity(SparseVoxelOctree svo, int nodeIndex)
{
    return SVOGetNodeData(svo, nodeIndex, svo.dataDim - 1);
}

float3 ComputeNodeColor(SparseVoxelOctree svo, int nodeIndex, float shBasis[25])
{
    float3 tmp = float3(0.0, 0.0, 0.0);

    for (int i = 0; i < svo.basisDim; ++i) {
        tmp.r += shBasis[i] * SVOGetNodeData(svo, nodeIndex,                  i);
        tmp.g += shBasis[i] * SVOGetNodeData(svo, nodeIndex,   svo.basisDim + i);
        tmp.b += shBasis[i] * SVOGetNodeData(svo, nodeIndex, 2*svo.basisDim + i);
    }

    float3 color = 1.0 / (1.0 + exp(-tmp));
    return pow(color, 2.2);
}

float ComputeDepth(float3 positionOS)
{
    float4 positionHCS = TransformObjectToHClip(positionOS);
    return positionHCS.z / positionHCS.w;
}

#endif // INPUT_INCLUDED
