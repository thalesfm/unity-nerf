using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using BlendMode = UnityEngine.Rendering.BlendMode;
using RenderQueue = UnityEngine.Rendering.RenderQueue;

namespace UnityNeRF
{
    public class RadianceFieldShaderGUI : ShaderGUI
    {
        protected Material material;
        protected MaterialEditor materialEditor;
        protected MaterialProperty[] properties;

        bool AlphaClip
        {
            get
            {
                MaterialProperty property = FindProperty("_AlphaClip", properties);
                return property.floatValue == 1.0f;
            }
            set
            {
                SetProperty("_AlphaClip", value ? 1.0f : 0.0f);
                SetKeyWord("_ALPHATEST_ON", value);
            }
        }

        BlendMode SrcBlend
        {
            set => SetProperty("_SrcBlend", (float)value);
        }

        BlendMode DstBlend
        {
            set => SetProperty("_DstBlend", (float)value);
        }

        bool ZWrite
        {
            set => SetProperty("_ZWrite", value ? 1.0f : 0.0f);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // base.OnGUI(materialEditor, properties);

            this.material = materialEditor.target as Material;
            this.materialEditor = materialEditor;
            this.properties = properties;

            // MaterialProperty alphaClipProp = FindProperty("_AlphaClip", properties);
            MaterialProperty alphaCutoffProp = FindProperty("_Cutoff", properties);
            MaterialProperty minTransmittance = FindProperty("_MinTransmittance", properties);
            MaterialProperty renderModeProp = FindProperty("_RenderMode", properties);
            // MaterialProperty blendProp = FindProperty("_Blend", properties);
            // MaterialProperty preserveSpecularProp = FindProperty("_BlendModePreserveSpecular", properties);

            // materialEditor.ShaderProperty(renderModeProp, renderModeProp.displayName);
            RenderMode renderMode = (RenderMode)materialEditor.PopupShaderProperty(
                renderModeProp,
                EditorGUIUtility.TrTextContent(renderModeProp.displayName),
                Enum.GetNames(typeof(RenderMode))
            );

            switch (renderMode)
            {
                case RenderMode.Opaque:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    AlphaClip = true;
                    SrcBlend = BlendMode.One;
                    DstBlend = BlendMode.Zero;
                    ZWrite = true;
                    break;
                case RenderMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    AlphaClip = false;
                    SrcBlend = BlendMode.SrcAlpha;
                    DstBlend = BlendMode.OneMinusSrcAlpha;
                    ZWrite = false;
                    break;
                // default:
                //     material.SetOverrideTag("RenderType", "");
            }

            if (AlphaClip)
            {
                // EditorGUI.BeginDisabledGroup(!AlphaClip);
                EditorGUI.indentLevel += 1;
                materialEditor.ShaderProperty(alphaCutoffProp, alphaCutoffProp.displayName);
                EditorGUI.indentLevel -= 1;
                // EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(minTransmittance, minTransmittance.displayName);
        }

        // MaterialProperty GetProperty(string name)
        // {
        //     return FindProperty(name, properties);
        // }

        void SetProperty(string name, float value)
        {            
            var property = FindProperty(name, properties);
            if (property == null)
                return;
            
            property.floatValue = value;
        }

        void SetKeyWord(string name, bool enabled)
        {
            CoreUtils.SetKeyword(material, name, enabled);
        }

        protected enum RenderMode
        {
            Opaque,
            Transparent,
        }

        // protected enum BlendingMode
        // {
        //     Alpha,
        //     Premultiply,
        //     Additive,
        //     Multiply,
        // }
    }
}
