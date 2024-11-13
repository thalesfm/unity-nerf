#ifndef VOLUME_CASTER_PASS_INCLUDED
#define VOLUME_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/VolumeRendering.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

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

Varyings VolumeShadowCasterVertex(Attributes input)
{
    Varyings output;
    output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    // output.positionHCS = GetShadowPositionHClip(input);
    output.rayOriginOS = input.positionOS.xyz;
    output.unRayDirectionOS = GetObjectSpaceRayDir(input.positionOS.xyz);
    return output;
}

half4 VolumeShadowCasterFragment(Varyings input, out float outDepth : SV_Depth) : SV_Target
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

        float opticalDepth = OpticalDepthHomogeneousMedium(extinction, _StepSize);
        float opacity = OpacityFromOpticalDepth(opticalDepth);
        float attenuation = 1 - opacity; // TransmittanceFromOpticalDepth(opticalDepth);
        
        depth += transmittance * opacity * t;
        transmittance *= attenuation;

        if (transmittance <= _MinTransmittance)
        {
            depth *= 1.0 / (1.0 - transmittance);
            transmittance = 0.0;
            break;
        }
    }

    if (transmittance > 0.0)
    {
        depth *= 1.0 / (1.0 - transmittance);
    }

    float alpha = 1.0 - transmittance;
#if defined(_ALPHATEST_ON) // Opaque
    clip(alpha - _Cutoff);
#else // Transparent
    #if defined(VOLUME_RENDERING_SEMITRANSPARENT_SHADOWS_ON)
    half dither = InterleavedGradientNoise(input.positionHCS.xy, 0);
    clip(alpha - dither);
    #endif
#endif

    float3 finalPositionOS = input.rayOriginOS + depth * normalize(input.unRayDirectionOS);
    outDepth = ComputeDepth(finalPositionOS);
    return 0.0;
}

#endif // VOLUME_CASTER_PASS_INCLUDED
