#ifndef UNIVERSAL_FORWARD_PASS_INCLUDED
#define UNIVERSAL_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/AxisAlignedBoundingBox.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/SphericalHarmonics.hlsl"

struct Attributes
{
    float4 positionOS       : POSITION;
};

struct Varyings
{
    float4 positionHCS                    : SV_POSITION;
    noperspective float3 rayOriginOS      : TEXCOORD0;
    noperspective float3 unRayDirectionOS : TEXCOORD1;
};

Varyings ForwardPassVertex(Attributes input)
{
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    float2 positionNDC = ComputeNormalizedDeviceCoordinates(output.positionHCS.xyz / output.positionHCS.w);
    float3 rayOriginWS = ComputeWorldSpacePosition(positionNDC, UNITY_NEAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
    output.rayOriginOS = TransformWorldToObject(rayOriginWS);
    // output.rayOriginOS = input.positionOS.xyz;
    float3 rayTargetWS = ComputeWorldSpacePosition(positionNDC, UNITY_RAW_FAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
    float3 rayTargetOS = TransformWorldToObject(rayTargetWS);
    output.unRayDirectionOS = rayTargetOS - output.rayOriginOS;
    return output;
}

half4 ForwardPassFragment(Varyings input, out float depth : SV_Depth) : SV_Target
{
    SparseVoxelOctree svo = GetSparseVoxelOctree();
    float3 rayDirectionOS = normalize(input.unRayDirectionOS);
    float3 positionOS = input.rayOriginOS;

    // Transform from Unity's coordinate frame (XZY) to XYZ
    rayDirectionOS = rayDirectionOS.xzy;
    positionOS = positionOS.xzy;

    float t;
    if (IntersectPointBox(positionOS / _Scale)) {
        t = 0.0;
    } else {
        t = IntersectRayBox(positionOS / _Scale, rayDirectionOS);
        if (t == -1.0) {
            discard;
        } else {
            t *= _Scale;
            positionOS += t * rayDirectionOS;
        }
    }
            
    float shBasis[25];
    EvalSH25(rayDirectionOS, shBasis);

    float3 finalColor = float3(0.0, 0.0, 0.0);
    float finalDepth = 0.0;
    float transmittance = 1.0;

    for (int i = 0; i < _MaxSteps; ++i) {
        t += _StepSize;
        positionOS += _StepSize * rayDirectionOS;
        int nodeIndex = SVOGetNodeIndexAt(svo, positionOS / _Scale); // A bit of a hack
        float density = max(GetNodeDensity(svo, nodeIndex), 0.0);

        if (density == 0.0) {
            continue;
        }

        float3 voxelColor = ComputeNodeColor(svo, nodeIndex, shBasis);
        float attenuation = min(exp(-_StepSize * density), 1.f);
        finalColor += transmittance * (1.0 - attenuation) * voxelColor;
        finalDepth += transmittance * (1.0 - attenuation) * t;
        transmittance *= attenuation;

        if (transmittance <= _MinTransmittance) {
            finalColor *= 1.0 / (1.0 - transmittance);
            finalDepth *= 1.0 / (1.0 - transmittance);
            transmittance = 0.0;
            break;
        }
    }

    if (transmittance > 0.0) {
        finalColor *= 1.0 / (1.0 - transmittance);
        finalDepth *= 1.0 / (1.0 - transmittance);
    }

#if defined(_ALPHATEST_ON)
    clip(1.0 - transmittance - _Cutoff);
#endif

    float3 finalPositionOS = input.rayOriginOS + finalDepth * normalize(input.unRayDirectionOS);
    depth = ComputeDepth(finalPositionOS);
    // finalColor = finalColor * finalColor; // Hack for gamma correction
    return half4(finalColor, 1 - transmittance);
}

#endif // UNIVERSAL_FORWARD_PASS_INCLUDED
