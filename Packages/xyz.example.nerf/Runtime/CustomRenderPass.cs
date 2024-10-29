using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

class CustomRenderPass : ScriptableRenderPass
{
    // private static readonly int horizontalBlurId = Shader.PropertyToID("_HorizontalBlur");
    // private static readonly int verticalBlurId = Shader.PropertyToID("_VerticalBlur");

    private CustomPassSettings settings;
    private Material material;

    public CustomRenderPass(Material material, CustomPassSettings settings)
    {
        this.material = material;
        this.settings = settings;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // var sortingSettings = new SortingSettings(renderingData.cameraData.camera)
        // {
        //     criteria = SortingCriteria.CommonTransparent
        // };

        // var drawingSettings = new DrawingSettings();

        // var filteringSettings = new FilteringSettings();
        // filteringSettings.renderQueueRange = new RenderQueueRange()

        // context.DrawRenderers(
        //     renderingData.cullResults,
        //     ref drawingSettings,
        //     ref filteringSettings
        // );

        //Execute the command buffer and release it back to the pool.
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
    #if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            UnityEngine.Object.Destroy(material);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
    #else
        Object.Destroy(material);
    #endif
    }
}
