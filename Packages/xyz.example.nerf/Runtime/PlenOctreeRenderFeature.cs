using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityNeRF
{
    [Serializable]
    public class PlenOctreeRenderSettings
    {
        public LayerMask layerMask = (LayerMask)0;
        // public int maxSteps;
        // public int stepSize;
        // public bool semitransparentShadows;
    }

    [DisallowMultipleRendererFeature("PlenOctree Rendering")]
    public class PlenOctreeRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private PlenOctreeRenderSettings settings = new();

        private PlenOctreeRenderPass opaqueRenderPass;
        private PlenOctreeRenderPass transparentRenderPass;

        public override void Create()
        {
            if (opaqueRenderPass == null)
                opaqueRenderPass = new PlenOctreeRenderPass(settings, false);
            if (transparentRenderPass == null)
                transparentRenderPass = new PlenOctreeRenderPass(settings, true);
            
            opaqueRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            transparentRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        protected override void Dispose(bool disposing)
        {
            opaqueRenderPass?.Dispose();
            opaqueRenderPass = null;

            transparentRenderPass?.Dispose();
            transparentRenderPass = null;
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            opaqueRenderPass.Setup(ref renderer);
            transparentRenderPass.Setup(ref renderer);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
                return;

            renderer.EnqueuePass(opaqueRenderPass);
            renderer.EnqueuePass(transparentRenderPass);
        }
    }
}
