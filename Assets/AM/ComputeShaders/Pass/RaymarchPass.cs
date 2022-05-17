using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AM.ComputeShaders.Pass
{
    public class RaymarchPass : ScriptableRenderPass
    {
        private const string PROFILER_TAG = "Raymarch Pass";

        private readonly RaymarchFeature.Settings _settings;
        private RenderTargetIdentifier _rayMarchBuffer;
        private readonly ComputeShader _raymarchShader;
        private readonly int _targetID = Shader.PropertyToID("_rayMarchBuffer");

        private static readonly int MaxStepsProperty = Shader.PropertyToID("_MaxSteps");
        private static readonly int MaxDistanceProperty = Shader.PropertyToID("_MaxDistance");
        private static readonly int SurfaceDistanceProperty = Shader.PropertyToID("_Surf_Distance");

        private int _renderTextureWidth;
        private int _renderTextureHeight;


        public RaymarchPass(RaymarchFeature.Settings settings)
        {
            _settings = settings;

            renderPassEvent = _settings.renderPassEvent;
            _raymarchShader = _settings.raymarchShader;

            _raymarchShader.SetInt(MaxStepsProperty, _settings.maxSteps);
            _raymarchShader.SetInt(MaxDistanceProperty, _settings.maxDistance);
            _raymarchShader.SetFloat(SurfaceDistanceProperty, _settings.surfDistance);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.enableRandomWrite = true;

            cmd.GetTemporaryRT(_targetID, descriptor);
            _rayMarchBuffer = new RenderTargetIdentifier(_targetID);

            _renderTextureWidth = descriptor.width;
            _renderTextureHeight = descriptor.height;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                if (renderingData.cameraData.isSceneViewCamera)
                    return;

                // var mainKernel = _raymarchShader.FindKernel(_settings.kernelName);
                // _raymarchShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
                cmd.Blit(renderingData.cameraData.targetTexture, _rayMarchBuffer);

                cmd.SetComputeTextureParam(_raymarchShader, 0, "Source", _rayMarchBuffer);
                cmd.SetComputeTextureParam(_raymarchShader, 0, "Destination", _targetID);
                cmd.SetComputeMatrixParam(_raymarchShader, "_CameraToWorld",
                    renderingData.cameraData.camera.cameraToWorldMatrix);
                cmd.SetComputeMatrixParam(_raymarchShader, "_CameraToInverseProjection",
                    renderingData.cameraData.camera.projectionMatrix.inverse);

                int threadGroupsX = Mathf.CeilToInt(_renderTextureWidth / 8f);
                int threadGroupsY = Mathf.CeilToInt(_renderTextureHeight / 8f);

                cmd.DispatchCompute(_raymarchShader, 0, threadGroupsX, threadGroupsY, 1);
                cmd.Blit(_rayMarchBuffer, renderingData.cameraData.renderer.cameraColorTarget);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                // // Init ();
                // // buffersToDispose = new List<ComputeBuffer> ();
                //
                // // InitRenderTexture ();
                // // CreateScene ();
                // // SetParameters ();
                //
                // _raymarchShader.SetTexture(0, "Source", source);
                // _raymarchShader.SetTexture(0, "Destination", target);
                //
                // int threadGroupsX = Mathf.CeilToInt(_renderTextureWidth / 8.0f);
                // int threadGroupsY = Mathf.CeilToInt(_renderTextureHeight / 8.0f);
                // _raymarchShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
                //
                // Graphics.Blit(target, destination);
                //
                // // foreach (var buffer in buffersToDispose) {
                // //     buffer.Dispose ();
                // // }
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            // Since we created a temporary render texture in OnCameraSetup, we need to release the memory here to avoid a leak.
            cmd.ReleaseTemporaryRT(_targetID);
        }
    }
}