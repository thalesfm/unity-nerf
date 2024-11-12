using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityNeRF
{
    [Serializable]
    public class VolumetricRenderSettings
    {
        public int MaxSteps = 1000;
        public float StepSize = 0.003f;
        public ShadowSettings ShadowSettings;
    }

    [Serializable]
    public class ShadowSettings
    {
        public bool SemitransparentShadows;
    }

    [DisallowMultipleRendererFeature("Volumetric Rendering")]
    public class VolumetricRenderFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private VolumetricRenderSettings settings = new();

        private SetupShadowCasterPass setupPass;
        private DrawVolumetricsPass opaqueRenderPass;
        private DrawVolumetricsPass transparentRenderPass;

        public override void Create()
        {
            setupPass ??= new SetupShadowCasterPass(settings);
            opaqueRenderPass ??= new DrawVolumetricsPass(settings, false);
            transparentRenderPass ??= new DrawVolumetricsPass(settings, true);
            
            setupPass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
            opaqueRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            transparentRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        // protected override void Dispose(bool disposing)
        // { }

        // public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        // { }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
                return;

            renderer.EnqueuePass(setupPass);
            renderer.EnqueuePass(opaqueRenderPass);
            renderer.EnqueuePass(transparentRenderPass);
        }
    }
}
