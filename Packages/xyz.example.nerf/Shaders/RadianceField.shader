Shader "Unlit/RadianceField"
{
    Properties
    {
        _Treshold("Transmittance Treshold", Range(0.0, 1.0)) = 0.5
        _MinTransmittance("Min. Transmittance", Range(0.0, 1.0)) = 0.05

        // TODO: Remove
        [HideInInspector] _Scale("Scale", Float) = 1.0
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

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma target 2.0

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

            HLSLPROGRAM
            #pragma target 2.0
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldInput.hlsl"
            #include "Packages/xyz.example.nerf/Shaders/RadianceFieldShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
