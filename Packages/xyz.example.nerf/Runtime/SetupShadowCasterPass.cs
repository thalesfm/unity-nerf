using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityNeRF
{
    internal class SetupShadowCasterPass : ScriptableRenderPass
    {
        private static readonly ProfilingSampler m_ProfilingSampler = new("SetupShadowCasterPass");

        private VolumeRenderingSettings settings;

        public SetupShadowCasterPass(VolumeRenderingSettings settings)
        {
            base.profilingSampler = new ProfilingSampler(nameof(SetupShadowCasterPass));
            this.settings = settings;
            // this.renderPassEvent = renderPassEvent;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.SetGlobalInt("_MaxSteps", settings.MaxSteps);
            cmd.SetGlobalFloat("_StepSize", settings.StepSize);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            ExecutePass(cmd, ref renderingData, settings.ShadowSettings.SemitransparentShadows);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public static void ExecutePass(CommandBuffer cmd, ref RenderingData renderingData, bool enableDithering)
        {
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                CoreUtils.SetKeyword(cmd, "VOLUME_RENDERING_SEMITRANSPARENT_SHADOWS_ON", enableDithering);
                
                // Matrix4x4 viewMatrix = renderingData.cameraData.GetViewMatrix();
                // Matrix4x4 projectionMatrix = renderingData.cameraData.GetGPUProjectionMatrix();
                // Matrix4x4 inverseViewMatrix = Matrix4x4.Inverse(viewMatrix);
                // Matrix4x4 inverseProjectionMatrix = Matrix4x4.Inverse(projectionMatrix);
                // Matrix4x4 inverseViewProjection = inverseViewMatrix * inverseProjectionMatrix;

                // Matrix4x4 viewProjection = Shader.GetGlobalMatrix("unity_MatrixVP");
                // Matrix4x4 inverseViewProjection = Matrix4x4.Inverse(viewProjection);

                // cmd.SetGlobalMatrix("_MatrixInvVP", inverseViewProjection);
            }
        }
    }
}
