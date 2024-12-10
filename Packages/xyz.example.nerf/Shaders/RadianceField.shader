Shader "Universal Render Pipeline/Radiance Field"
{
    Properties
    {
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        // _AlphaCutoffShadow("_AlphaCutoffShadow", Range(0.0, 1.0)) = 0.5

        _RenderMode("Rendering Mode", Float) = 0.0
        // _Blend("Blend", Float) = 0.0
        // _Cull("__cull", Float) = 2.0
        /* [Toggle(_ALPHATEST_ON)] */ _AlphaClip("Alpha Clip", Float) = 0.0
        /* [Enum(UnityEngine.Rendering.BlendMode)] */ [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
        /* [Enum(UnityEngine.Rendering.BlendMode)] */ [HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
        // [HideInInspector] _SrcBlendAlpha("Src Blend Alpha", Float) = 1.0
        // [HideInInspector] _DstBlendAlpha("Dst Blend Alpha", Float) = 0.0
		/* [Enum(Off, 0, On, 1)] */ [HideInInspector] _ZWrite ("_ZWrite", Float) = 1.0
        // [HideInInspector] [Toggle] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        // [HideInInspector] _AlphaToMask("_AlphaToMask", Float) = 0.0

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

        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        // Cull [_Cull]
        // AlphaToMask [_AlphaToMask]

        Pass
        {
            Name "VolumeForward"
            Tags { "LightMode" = "VolumeForward" }

            HLSLPROGRAM
            #pragma target 4.5

            #pragma vertex VolumeForwardVertex
            #pragma fragment VolumeForwardFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldInput.hlsl"
            #include "Packages/xyz.example.nerf/Shaders/VolumeForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.5

            #pragma vertex VolumeShadowCasterVertex
            #pragma fragment VolumeShadowCasterFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_fragment _ VOLUME_RENDERING_SEMITRANSPARENT_SHADOWS_ON

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldInput.hlsl"
            #include "Packages/xyz.example.nerf/Shaders/VolumeShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityNeRF.Editor.RadianceFieldShaderGUI"
}
