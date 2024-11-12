#ifndef SHADOW_CASTER_PASS_INCLUDED
#define SHADOW_CASTER_PASS_INCLUDED

// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/SphericalHarmonics.hlsl"

float3 _LightDirection;
float3 _LightPosition;

struct Attributes
{
    float4 positionOS       : POSITION;
    float4 normalOS         : NORMAL;
};

struct Varyings
{
    float4 positionHCS      : SV_POSITION;
    float3 rayOriginOS      : TEXCOORD0;
    float3 unRayDirectionOS : TEXCOORD1;
};

// float4 GetShadowPositionHClip(Attributes input)
// {
//     float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
//     float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
// #if _CASTING_PUNCTUAL_LIGHT_SHADOW
//     float3 lightDirectionWS = normalize(_LightPosition - positionWS);
// #else
//     float3 lightDirectionWS = _LightDirection;
// #endif
//     float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
// #if UNITY_REVERSED_Z
//     positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
// #else
//     positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
// #endif
//     return positionCS;
// }

float3 GetObjectSpaceRayDir(float3 positionOS)
{
#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    return positionOS - TransformWorldToObject(_LightPosition);
#else
    return -TransformWorldToObjectNormal(_LightDirection);
#endif
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    // output.positionHCS = GetShadowPositionHClip(input);
    output.rayOriginOS = input.positionOS.xyz;
    output.unRayDirectionOS = GetObjectSpaceRayDir(input.positionOS.xyz);
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

    float t = 0.0;
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
