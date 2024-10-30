Shader "Unlit/RadianceField"
{
    Properties
    {
        [Toggle(_ALPHATEST_ON)] _Clipping("Alpha Clipping", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _MinTransmittance("Min. Transmittance", Range(0.0, 1.0)) = 0.05

        // TODO: Remove
        [HideInInspector] _Scale("Scale", Float) = 0.5
        // [HideInInspector] _SVOWidth("a", Integer) = 0
        // [HideInInspector] _SVOHeight("b", Integer) = 0
        // [HideInInspector] _SVODepth("c", Integer) = 0
        // [HideInInspector] _SVOBasisDim("d", Integer) = 0
        // [HideInInspector] _SVODataDim("e", Integer) = 0
        // [HideInInspector] _SVOMaxLevel("f", Integer) = 0

        /* [HideInInspector] */ [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5.0 // SrcAlpha
		/* [HideInInspector] */ [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10.0 // OneMinusSrcAlpha
		/* [HideInInspector] */ [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1.0
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
}
