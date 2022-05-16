using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AM.ComputeShaders.Pass
{
    public class RayMarchPass : ScriptableRenderPass
    {
        private const string PROFILER_TAG = "Raymarch Pass";

        private RaymarchFeature.PassSettings _passSettings;
        private RenderTargetIdentifier rayMarchBuffer;
        private ComputeShader _raymarchShader;

        private static readonly int MaxStepsProperty = Shader.PropertyToID("_MaxSteps");
        private static readonly int MaxDistanceProperty = Shader.PropertyToID("_MaxDistance");
        private static readonly int SurfaceDistanceProperty = Shader.PropertyToID("_Surf_Distance");

        public RayMarchPass(RaymarchFeature.PassSettings passSettings)
        {
            _passSettings = passSettings;

            renderPassEvent = _passSettings.renderPassEvent;
            _raymarchShader = _passSettings.raymarchShader;

            _raymarchShader.SetInt(MaxStepsProperty, _passSettings.maxSteps);
            _raymarchShader.SetInt(MaxDistanceProperty, _passSettings.maxDistance);
            _raymarchShader.SetFloat(SurfaceDistanceProperty, _passSettings.surfDistance);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                // Blit from the color buffer to a temporary buffer and back. This is needed for a two-pass shader.
                // Blit(cmd, temporaryBuffer, rayMarchBuffer, _raymarchShader, 0); // shader pass 0
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }
    }
}