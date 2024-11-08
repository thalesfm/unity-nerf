using System;
using Codice.CM.Common.Matcher;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityNeRF
{
    class PlenOctreeRenderPass : ScriptableRenderPass
    {
        private static readonly ShaderTagId shaderTagId = new("Volumetric");

        private PlenOctreeRenderSettings settings;
        private bool transparent;
        // private ScriptableRenderer renderer;
        private new ProfilingSampler profilingSampler = new(nameof(PlenOctreeRenderPass));

        public PlenOctreeRenderPass(PlenOctreeRenderSettings settings, bool transparent)
        {
            this.settings = settings;
            this.transparent = transparent;
        }

        public void Dispose()
        {
            // CoreUtils.Destroy(errorMaterial);
        }

        internal void Setup(ref ScriptableRenderer renderer)
        {
            // this.renderer = renderer;
        }

        // public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        // { }

        // public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        // {
        //     GlobalKeyword semitransparentShadows = new("_SemitransparentShadows");
        //     cmd.SetKeyword(semitransparentShadows, settings.semitransparentShadows);

        //     RTHandle cameraDepthTargetHandle = renderer.cameraDepthTargetHandle;
        // }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            SortingCriteria sortingCriteria = transparent
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;

            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, sortingCriteria);

            RenderQueueRange renderQueueRange = transparent
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            var filterSettings = new FilteringSettings(renderQueueRange);

            CommandBuffer cmd = CommandBufferPool.Get("PlenOctreeRenderPass");
            using (new ProfilingScope(cmd, profilingSampler))
            {
                // Flush command-buffer before rendering
                // context.ExecuteCommandBuffer(cmd);
                // cmd.Clear();

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSettings);

                // Execute the command buffer and release it back to the pool.
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
