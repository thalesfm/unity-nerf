Shader "Universal Render Pipeline/Radiance Field"
{
    Properties
    {
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _RenderMode("Rendering Mode", Float) = 0.0
        // _Blend("Blend", Float) = 0.0
        // _Cull("__cull", Float) = 2.0
        /* [Toggle(_ALPHATEST_ON)] */ _AlphaClip("Alpha Clip", Float) = 0.0
        /* [Enum(UnityEngine.Rendering.BlendMode)] */ [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0 // 5.0 // SrcAlpha
        /* [Enum(UnityEngine.Rendering.BlendMode)] */ [HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0 // 10.0 // OneMinusSrcAlpha
        // [HideInInspector] _SrcBlendAlpha("Src Blend Alpha", Float) = 1.0
        // [HideInInspector] _DstBlendAlpha("Dst Blend Alpha", Float) = 0.0
		/* [Enum(Off, 0, On, 1)] */ [HideInInspector] _ZWrite ("_ZWrite", Float) = 1.0
        // [HideInInspector] [Toggle] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        // [HideInInspector] _AlphaToMask("Alpha to Mask", Float) = 0.0

        // [ToggleUI] _ReceiveShadows("Receive Shadows", Float) = 1.0

        _MinTransmittance("Min. Transmittance", Range(0.0, 1.0)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        // Blend SrcAlpha OneMinusSrcAlpha
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull Front

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma target 4.5

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma vertex ForwardPassVertex
            #pragma fragment ForwardPassFragment

            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldInput.hlsl"
            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            // ZTest LEqual
            ColorMask 0
            // Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            #define _ALPHATEST_ON 1
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldInput.hlsl"
            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityNeRF.RadianceFieldShaderGUI"
}
