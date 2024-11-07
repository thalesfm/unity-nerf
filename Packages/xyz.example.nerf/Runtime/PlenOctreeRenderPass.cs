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
        private static readonly ShaderTagId shaderTagId = new("UniversalForward");

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

            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;

            // Culling with the proper layer mask doesn't seem to be necessary...
            // if (!TryCull(context, camera, out CullingResults cullResults))
            //     return;
            CullingResults cullResults = renderingData.cullResults;

            RenderQueueRange renderQueueRange = transparent
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            var filterSettings = new FilteringSettings(renderQueueRange)
            {
                layerMask = settings.layerMask,
            };

            CommandBuffer cmd = CommandBufferPool.Get("PlenOctreeRenderPass");
            // CommandBuffer cmd = renderingData.commandBuffer;
            using (new ProfilingScope(cmd, profilingSampler))
            {
                // Flush command-buffer before rendering
                // context.ExecuteCommandBuffer(cmd);
                // cmd.Clear();

                context.DrawRenderers(cullResults, ref drawingSettings, ref filterSettings);

                // Execute the command buffer and release it back to the pool.
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        private bool TryCull(ScriptableRenderContext context, Camera camera, out CullingResults cullResults)
        {
            // LayerMask cullingMask = camera.cullingMask;
            // camera.cullingMask = settings.layerMask;
            bool success = camera.TryGetCullingParameters(out ScriptableCullingParameters parameters);
            // camera.cullingMask = cullingMask;

            if (!success)
            {
                cullResults = new CullingResults();
                return false;
            }

            parameters.cullingMask = (uint)settings.layerMask.value;
            cullResults = context.Cull(ref parameters);
            return true;
        }
    }
}
