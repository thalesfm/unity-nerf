using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private CustomPassSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private CustomRenderPass customRenderPass;

    public override void Create()
    {
        // if (shader == null) return;

        // material = new Material(shader);
        customRenderPass = new CustomRenderPass(material, settings);
        customRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (customRenderPass != null) customRenderPass.Dispose();
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
            Destroy(material);
        else
            DestroyImmediate(material);
#else
        Destroy(material);
#endif
    }
}

[Serializable]
public class CustomPassSettings
{
    [Range(0, 1.0f)] public float SomeValue;
}
