#ifndef VOLUME_FORWARD_PASS_INCLUDED
#define VOLUME_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/VolumeRendering.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/xyz.example.nerf/ShaderLibrary/VolumeDebug.hlsl"

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

Varyings VolumeForwardVertex(Attributes input)
{
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    output.rayOriginOS = input.positionOS.xyz;
    output.unRayDirectionOS = GetObjectSpaceRayDir(input.positionOS.xyz);
    return output;
}

half4 VolumeForwardFragment(Varyings input, out float outDepth : SV_Depth) : SV_Target
{    
    float3 rayDirectionOS = normalize(input.unRayDirectionOS);
    float3 positionOS = input.rayOriginOS;

    // Transform from Unity's coordinate frame (XZY) to XYZ
    rayDirectionOS = rayDirectionOS.xzy;
    positionOS = positionOS.xzy;

    VolumeData volume;
    InitializeVolumeData(positionOS, rayDirectionOS, volume);

    float t = 0.0;
    float3 color = float3(0.0, 0.0, 0.0);
    float depth = 0.0;
    float transmittance = 1.0;

    for (int i = 0; i < _MaxSteps; ++i)
    {
        t += _StepSize;
        positionOS += _StepSize * rayDirectionOS;

        float extinction = SampleExtinction(volume, positionOS / _Scale);
        if (extinction <= 0.0)
            continue;

        float3 radiance = SampleRadiance(volume, positionOS / _Scale);
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
    float alpha = 1.0 - transmittance;
    clip(alpha - _Cutoff);
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

#endif // VOLUME_FORWARD_PASS_INCLUDED
