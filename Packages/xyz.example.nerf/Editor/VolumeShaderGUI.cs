using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using BlendMode = UnityEngine.Rendering.BlendMode;
using RenderQueue = UnityEngine.Rendering.RenderQueue;

namespace UnityNeRF.Editor
{
    public class VolumeShaderGUI : ShaderGUI
    {
        // protected Material material;
        protected MaterialEditor materialEditor;

        protected MaterialProperty alphaCutoffProp;
        protected MaterialProperty alphaClipProp;
        protected MaterialProperty renderModeProp;
        protected MaterialProperty srcBlendProp;
        protected MaterialProperty dstBlendProp;
        protected MaterialProperty zWriteProp;
        protected MaterialProperty minTransmittanceProp;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            // this.material = materialEditor.target as Material;
            Material material = materialEditor.target as Material;
            this.materialEditor = materialEditor;

            EditorGUI.BeginChangeCheck();
            RenderMode renderMode = (RenderMode)materialEditor.PopupShaderProperty(
                renderModeProp,
                EditorGUIUtility.TrTextContent(renderModeProp.displayName),
                Enum.GetNames(typeof(RenderMode))
            );
            bool renderModeChanged = EditorGUI.EndChangeCheck();

            if (renderModeChanged)
            {
                switch (renderMode)
                {
                    case RenderMode.Opaque:
                        material.SetOverrideTag("RenderType", "TransparentCutout");
                        material.renderQueue = (int)RenderQueue.AlphaTest;
                        alphaClipProp.floatValue = 1.0f;
                        CoreUtils.SetKeyword(material, "_ALPHATEST_ON", true);
                        SetMaterialSrcDstBlendProperties(material, BlendMode.One, BlendMode.Zero);
                        SetMaterialZWriteProperty(material, true);
                        break;
                    case RenderMode.Transparent:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.renderQueue = (int)RenderQueue.Transparent;
                        alphaClipProp.floatValue = 0.0f;
                        CoreUtils.SetKeyword(material, "_ALPHATEST_ON", false);
                        SetMaterialSrcDstBlendProperties(material, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha);
                        SetMaterialZWriteProperty(material, false);
                        break;
                }
            }

            if (alphaClipProp.floatValue == 1.0f)
            {
                EditorGUI.indentLevel += 1;
                materialEditor.ShaderProperty(alphaCutoffProp, alphaCutoffProp.displayName);
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            materialEditor.ShaderProperty(minTransmittanceProp, minTransmittanceProp.displayName);
            EditorGUI.indentLevel -= 1;
        }

        protected static void SetMaterialSrcDstBlendProperties(Material material, BlendMode srcBlend, BlendMode dstBlend)
        {
            if (material.HasProperty(Property.SrcBlend))
                material.SetFloat(Property.SrcBlend, (float)srcBlend);

            if (material.HasProperty(Property.DstBlend))
                material.SetFloat(Property.DstBlend, (float)dstBlend);
        }

        protected static void SetMaterialZWriteProperty(Material material, bool zwriteEnabled)
        {
            if (material.HasProperty(Property.ZWrite))
                material.SetFloat(Property.ZWrite, zwriteEnabled ? 1.0f : 0.0f);
        }

        protected void FindProperties(MaterialProperty[] properties)
        {
            alphaCutoffProp = FindProperty("_Cutoff", properties);
            alphaClipProp = FindProperty(Property.AlphaClip, properties);
            renderModeProp = FindProperty("_RenderMode", properties);
            srcBlendProp = FindProperty(Property.SrcBlend, properties);
            dstBlendProp = FindProperty(Property.DstBlend, properties);
            zWriteProp = FindProperty(Property.ZWrite, properties);
            minTransmittanceProp = FindProperty("_MinTransmittance", properties);
        }

        protected enum RenderMode
        {
            Opaque,
            Transparent,
        }

        // protected enum BlendMode
        // {
        //     Alpha,
        //     Premultiply,
        //     Additive,
        //     Multiply,
        // }

        protected static class Property
        {
            public const string RenderMode = "_RenderMode";
            public const string AlphaClip = "_AlphaClip";
            // public const string AlphaToMask = "_AlphaToMask";
            public const string SrcBlend = "_SrcBlend";
            public const string DstBlend = "_DstBlend";
            // public const string SrcBlendAlpha = "_SrcBlendAlpha";
            // public const string DstBlendAlpha = "_DstBlendAlpha";
            public const string ZWrite = "_ZWrite";
            // public const string CullMode = "_Cull";
            // public const string CastShadows = "_CastShadows";
            // public const string ReceiveShadows = "_ReceiveShadows";
            // public const string QueueOffset = "_QueueOffset";
        }
    }
}
