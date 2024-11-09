using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityNeRF
{
    internal class SetupVolumetricShadowCasterPass : ScriptableRenderPass
    {
        private static readonly ProfilingSampler m_ProfilingSampler = new("PlenOctreeSetupPass");

        private VolumetricRenderSettings settings;

        public SetupVolumetricShadowCasterPass(VolumetricRenderSettings settings)
        {
            base.profilingSampler = new ProfilingSampler(nameof(SetupVolumetricShadowCasterPass));
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
            ExecutePass(cmd, settings.ShadowSettings.SemitransparentShadows);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public static void ExecutePass(CommandBuffer cmd, bool enableDithering)
        {
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                CoreUtils.SetKeyword(cmd, "RADIANCE_FIELDS_SEMITRANSPARENT_SHADOWS_ON", enableDithering);
            }
        }
    }
}
