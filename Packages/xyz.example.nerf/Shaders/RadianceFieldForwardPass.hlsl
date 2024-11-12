#ifndef UNIVERSAL_FORWARD_PASS_INCLUDED
#define UNIVERSAL_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/VolumeRendering.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/Debug.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/SphericalHarmonics.hlsl"

struct Attributes
{
    float4 positionOS       : POSITION;
};

struct Varyings
{
    float4 positionHCS      : SV_POSITION;
    float3 rayOriginOS      : TEXCOORD0;
    float3 unRayDirectionOS : TEXCOORD1;
};

struct CustomData
{
    SparseVoxelOctree svo;
    float shBasis[25];
};

void InitializeCustomData(Varyings input, inout CustomData customData)
{
    customData.svo = GetSparseVoxelOctree();
    EvalSH25(normalize(input.unRayDirectionOS).xzy, customData.shBasis);
}

float SampleExtinction(CustomData customData, float3 position)
{
    int nodeIndex = SVOGetNodeIndexAt(customData.svo, position);
    return GetNodeDensity(customData.svo, nodeIndex);
}

float3 SampleRadiance(CustomData customData, float3 position)
{
    int nodeIndex = SVOGetNodeIndexAt(customData.svo, position);
    return ComputeNodeColor(customData.svo, nodeIndex, customData.shBasis);
}

float3 GetObjectSpaceRayDir(float3 positionOS)
{
    if (IsPerspectiveProjection())
    {
        return positionOS - TransformWorldToObject(GetCurrentViewPosition());
    }
    else
    {
        return -TransformWorldToObjectNormal(-GetViewForwardDir());
    }
}

Varyings ForwardPassVertex(Attributes input)
{
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    output.rayOriginOS = input.positionOS.xyz;
    output.unRayDirectionOS = GetObjectSpaceRayDir(input.positionOS.xyz);
    return output;
}

half4 ForwardPassFragment(Varyings input, out float outDepth : SV_Depth) : SV_Target
{    
    float3 rayDirectionOS = normalize(input.unRayDirectionOS);
    float3 positionOS = input.rayOriginOS;

    // Transform from Unity's coordinate frame (XZY) to XYZ
    rayDirectionOS = rayDirectionOS.xzy;
    positionOS = positionOS.xzy;

    CustomData customData;
    InitializeCustomData(input, customData);

    float t = 0.0;
    float3 color = float3(0.0, 0.0, 0.0);
    float depth = 0.0;
    float transmittance = 1.0;

    for (int i = 0; i < _MaxSteps; ++i)
    {
        t += _StepSize;
        positionOS += _StepSize * rayDirectionOS;

        float extinction = SampleExtinction(customData, positionOS / _Scale);
        if (extinction <= 0.0)
            continue;

        float3 radiance = SampleRadiance(customData, positionOS / _Scale);
        float opticalDepth = OpticalDepthHomogeneousMedium(extinction, _StepSize);
        float opacity = OpacityFromOpticalDepth(opticalDepth);
        float attenuation = 1 - opacity; // TransmittanceFromOpticalDepth(opticalDepth);
        
        color += transmittance * opacity * radiance;
        depth += transmittance * opacity * t;
        transmittance *= attenuation;

        if (transmittance <= _MinTransmittance)
        {
            color *= 1.0 / (1.0 - transmittance);
            depth *= 1.0 / (1.0 - transmittance);
            transmittance = 0.0;
            break;
        }
    }

    if (transmittance > 0.0)
    {
        color *= 1.0 / (1.0 - transmittance);
        depth *= 1.0 / (1.0 - transmittance);
    }

#if defined(_ALPHATEST_ON)
    float opacity = 1.0 - transmittance;
    clip(opacity - _Cutoff);
#endif

    float3 finalPositionOS = input.rayOriginOS + depth * normalize(input.unRayDirectionOS);
    outDepth = ComputeDepth(finalPositionOS);

#if defined(DEBUG_DISPLAY)
    half4 debugColor;
    if (CanDebugOverrideOutputColor(debugColor))
        return debugColor;
#endif

    // finalColor = finalColor * finalColor; // Hack for gamma correction
    return half4(color, 1 - transmittance);
}

#endif // UNIVERSAL_FORWARD_PASS_INCLUDED
