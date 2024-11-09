#ifndef SHADOW_CASTER_PASS_INCLUDED
#define SHADOW_CASTER_PASS_INCLUDED

// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/AxisAlignedBoundingBox.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/SphericalHarmonics.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/Matrix.hlsl"

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

Varyings ShadowPassVertex(Attributes input)
{
    float4x4 inverseModel = UNITY_MATRIX_I_M;
    float4x4 inverseViewProj = inverse(UNITY_MATRIX_VP); // FIXME
    
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    float2 positionNDC = ComputeNormalizedDeviceCoordinates(output.positionHCS.xyz / output.positionHCS.w);
    float3 rayOriginWS = ComputeWorldSpacePosition(positionNDC, UNITY_NEAR_CLIP_VALUE, inverseViewProj);
    output.rayOriginOS = mul(inverseModel, float4(rayOriginWS, 1.0)).xyz;
    // output.rayOriginOS = input.positionOS.xyz;
    float3 rayTargetWS = ComputeWorldSpacePosition(positionNDC, UNITY_RAW_FAR_CLIP_VALUE, inverseViewProj);
    float3 rayTargetOS = mul(inverseModel, float4(rayTargetWS, 1.0)).xyz;
    output.unRayDirectionOS = rayTargetOS - output.rayOriginOS;
    return output;
}

half4 ShadowPassFragment(Varyings input, out float depth : SV_Depth) : SV_Target
{
    SparseVoxelOctree svo = GetSparseVoxelOctree();
    float3 rayDirectionOS = normalize(input.unRayDirectionOS);
    float3 positionOS = input.rayOriginOS /*/ _Scale*/;

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

    float transmittance = 1.0;
    float finalDepth = 0.0;

    for (int i = 0; i < _MaxSteps; ++i) {
        t += _StepSize;
        positionOS += _StepSize * rayDirectionOS;
        int nodeIndex = SVOGetNodeIndexAt(svo, positionOS / _Scale); // A bit of a hack
        float density = max(GetNodeDensity(svo, nodeIndex), 0.0);

        if (density == 0.0) {
            continue;
        }

        float attenuation = min(exp(-_StepSize * density), 1.f);
        finalDepth += transmittance * (1.0 - attenuation) * t;
        transmittance *= attenuation;

        if (transmittance <= _MinTransmittance) {
            finalDepth *= 1.0 / (1.0 - transmittance);
            transmittance = 0.0;
            break;
        }
    }

    if (transmittance > 0.0) {
        finalDepth *= 1.0 / (1.0 - transmittance);
    }

    half alpha = 1.0 - transmittance;

#if defined(_ALPHATEST_ON) // Opaque
    clip(alpha - _Cutoff);
#else // Transparent
    #if defined(RADIANCE_FIELDS_SEMITRANSPARENT_SHADOWS_ON)
    half dither = InterleavedGradientNoise(input.positionHCS.xy, 0);
    clip(alpha - dither);
    #endif
#endif

    float3 finalPositionOS = input.rayOriginOS + finalDepth * normalize(input.unRayDirectionOS);
    depth = ComputeDepth(finalPositionOS);
    return 0.0;
}

#endif // SHADOW_CASTER_PASS_INCLUDED
