Shader "Unlit/SparseVoxelOctree"
{
    Properties
    {
        _Scale("Scale", Float) = 1.0
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "AxisAlignedBoundingBox.hlsl"
    #include "Matrix.hlsl"
    #include "SparseVoxelOctree.hlsl"
    #include "SphericalHarmonics.hlsl"

    #define STEP_SIZE 0.003
    #define MAX_STEPS 1000

    CBUFFER_START(UnityPerMaterial)
        float _Scale;
        int _SVOWidth;
        int _SVOHeight;
        int _SVODepth;
        int _SVOMaxLevel;
        StructuredBuffer<int> _SVONodeChildren;
        StructuredBuffer<float> _SVONodeData;
    CBUFFER_END

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

    SparseVoxelOctree GetSparseVoxelOctree()
    {
        SparseVoxelOctree svo;
        svo.width = _SVOWidth;
        svo.height = _SVOHeight;
        svo.depth = _SVODepth;
        svo.dataDim = 49;
        svo.maxLevel = _SVOMaxLevel;
        svo.nodeChildren = _SVONodeChildren;
        svo.nodeData = _SVONodeData;
        return svo;
    }

    float GetNodeDensity(SparseVoxelOctree svo, int nodeIndex)
    {
        return SVOGetNodeData(svo, nodeIndex, 48);
    }

    float3 ComputeNodeColor(SparseVoxelOctree svo, int nodeIndex, float shBasis[16])
    {
        float3 tmp = float3(0.0, 0.0, 0.0);

        for (int i = 0; i < 16; ++i) {
            tmp.r += shBasis[i] * SVOGetNodeData(svo, nodeIndex, i);
            tmp.g += shBasis[i] * SVOGetNodeData(svo, nodeIndex, 16 + i);
            tmp.b += shBasis[i] * SVOGetNodeData(svo, nodeIndex, 32 + i);
        }

        return 1.0 / (1.0 + exp(-tmp));
    }

    float ComputeDepth(float3 positionOS)
    {
        float4 positionHCS = TransformObjectToHClip(positionOS);
        return positionHCS.z / positionHCS.w;
    }

    Varyings VertDefault(Attributes input)
    {
        Varyings output;
        output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
        float2 positionNDC = ComputeNormalizedDeviceCoordinates(output.positionHCS.xyz / output.positionHCS.w);
        float3 rayOriginWS = ComputeWorldSpacePosition(positionNDC, UNITY_NEAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
        output.rayOriginOS = TransformWorldToObject(rayOriginWS);
        float3 rayTargetWS = ComputeWorldSpacePosition(positionNDC, UNITY_RAW_FAR_CLIP_VALUE, UNITY_MATRIX_I_VP);
        float3 rayTargetOS = TransformWorldToObject(rayTargetWS);
        output.unRayDirectionOS = rayTargetOS - output.rayOriginOS;
        return output;
    }

    // TODO: Don't repeat yourself!
    Varyings VertShadowCaster(Attributes input)
    {
        float4x4 inverseModel = inverse(UNITY_MATRIX_M);
        float4x4 inverseViewProj = inverse(UNITY_MATRIX_VP);
        
        Varyings output;
        output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
        float2 positionNDC = ComputeNormalizedDeviceCoordinates(output.positionHCS.xyz / output.positionHCS.w);
        float3 rayOriginWS = ComputeWorldSpacePosition(positionNDC, UNITY_NEAR_CLIP_VALUE, inverseViewProj);
        output.rayOriginOS = mul(inverseModel, float4(rayOriginWS, 1.0)).xyz;
        float3 rayTargetWS = ComputeWorldSpacePosition(positionNDC, UNITY_RAW_FAR_CLIP_VALUE, inverseViewProj);
        float3 rayTargetOS = mul(inverseModel, float4(rayTargetWS, 1.0)).xyz;
        output.unRayDirectionOS = rayTargetOS - output.rayOriginOS;
        return output;
    }

    /* SV_DepthGreaterEqual */
    half4 FragDefault(Varyings input, out float depth : SV_Depth) : SV_Target
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
                
        float shBasis[16];
        EvalSH16(rayDirectionOS, shBasis);

        float3 finalColor = float3(0.0, 0.0, 0.0);
        float finalDepth = 0.0;
        float transmittance = 1.0;

        for (int i = 0; i < MAX_STEPS; ++i) {
            t += STEP_SIZE;
            positionOS += STEP_SIZE * rayDirectionOS;
            int nodeIndex = SVOGetNodeIndexAt(svo, positionOS / _Scale); // A bit of a hack
            float density = max(GetNodeDensity(svo, nodeIndex), 0.0);

            if (density == 0.0) {
                continue;
            }

            float3 voxelColor = ComputeNodeColor(svo, nodeIndex, shBasis);
            float attenuation = min(exp(-STEP_SIZE * density), 1.f);
            finalColor += transmittance * (1.0 - attenuation) * voxelColor;
            finalDepth += transmittance * (1.0 - attenuation) * t;
            transmittance *= attenuation;

            if (transmittance <= 1e-3) {
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

        float3 finalPositionOS = input.rayOriginOS + finalDepth * normalize(input.unRayDirectionOS);
        depth = ComputeDepth(finalPositionOS);
        finalColor = finalColor * finalColor; // Hack for gamma correction!
        return half4(finalColor, 1 - transmittance);
    }

    // TODO: Don't repeat yourself!
    half4 FragShadowCaster(Varyings input, out float depth : SV_Depth) : SV_Target
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

        for (int i = 0; i < MAX_STEPS; ++i) {
            t += STEP_SIZE;
            positionOS += STEP_SIZE * rayDirectionOS;
            int nodeIndex = SVOGetNodeIndexAt(svo, positionOS / _Scale); // A bit of a hack
            float density = max(GetNodeDensity(svo, nodeIndex), 0.0);

            if (density == 0.0) {
                continue;
            }

            float attenuation = min(exp(-STEP_SIZE * density), 1.f);
            finalDepth += transmittance * (1.0 - attenuation) * t;
            transmittance *= attenuation;

            if (transmittance <= 1e-3) {
                finalDepth *= 1.0 / (1.0 - transmittance);
                transmittance = 0.0;
                break;
            }
        }

        if (transmittance > 0.0) {
            finalDepth *= 1.0 / (1.0 - transmittance);
        }

        if (transmittance > 0.5) {
            discard;
        }

        float3 finalPositionOS = input.rayOriginOS + finalDepth * normalize(input.unRayDirectionOS);
        depth = ComputeDepth(finalPositionOS);
        return half4(1.0, 0.0, 1.0, 1.0);
    }

    ENDHLSL

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Cull Front
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment FragDefault

            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode"="ShadowCaster" "RenderPipeline"="UniversalPipeline" }
            Cull Front

            HLSLPROGRAM

            #pragma vertex VertShadowCaster
            #pragma fragment FragShadowCaster

            ENDHLSL
        }
    }
}
