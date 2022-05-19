using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AM.ComputeShaders
{
    public class RaymarchPass : ScriptableRenderPass
    {
        private const string PROFILER_TAG = "Raymarch Pass";

        private readonly RaymarchFeature.Settings _settings;
        private RenderTargetIdentifier _rayMarchBuffer;
        private readonly ComputeShader _raymarchShader;
        private readonly int _targetID = Shader.PropertyToID("_rayMarchBuffer");

        private int _renderTextureWidth;
        private int _renderTextureHeight;

        private readonly List<ComputeBuffer> _buffersToDispose = new();

        public RaymarchPass(RaymarchFeature.Settings settings)
        {
            _settings = settings;

            renderPassEvent = _settings.renderPassEvent;
            _raymarchShader = _settings.raymarchShader;

            CreateScene(settings.shapeData);
        }

        void CreateScene(ShapeData[] shapeData)
        {
            if (shapeData is not {Length: > 0}) return;
            Debug.Log(shapeData.Length);
            ComputeBuffer shapeBuffer = new ComputeBuffer(shapeData.Length, ShapeData.GetSize());
            shapeBuffer.SetData(shapeData);
            _raymarchShader.SetBuffer(0, "shapes", shapeBuffer);
            _raymarchShader.SetInt("numShapes", shapeData.Length);

            _buffersToDispose.Add(shapeBuffer);
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

        void SetParameters(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var mainLightIdx = renderingData.lightData.mainLightIndex;
            var lightSource = renderingData.lightData.visibleLights[mainLightIdx].light;

            bool lightIsDirectional = lightSource.type == LightType.Directional;

            cmd.SetComputeMatrixParam(_raymarchShader, "_CameraToWorld",
                renderingData.cameraData.camera.cameraToWorldMatrix);
            cmd.SetComputeMatrixParam(_raymarchShader, "_CameraToInverseProjection",
                renderingData.cameraData.camera.projectionMatrix.inverse);
            _raymarchShader.SetVector("_Light",
                (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
            _raymarchShader.SetBool("positionLight", !lightIsDirectional);

            cmd.SetComputeTextureParam(_raymarchShader, 0, "Source", renderingData.cameraData.renderer.cameraColorTarget);
            cmd.SetComputeTextureParam(_raymarchShader, 0, "Destination",_targetID );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler(PROFILER_TAG)))
            {
                // if (renderingData.cameraData.isSceneViewCamera)
                //     return;

                SetParameters(cmd, ref renderingData);

                var threadGroupsX = Mathf.CeilToInt(_renderTextureWidth / 8f);
                var threadGroupsY = Mathf.CeilToInt(_renderTextureHeight / 8f);

                cmd.DispatchCompute(_raymarchShader, 0, threadGroupsX, threadGroupsY, 1);

                cmd.Blit(_targetID, renderingData.cameraData.renderer.cameraColorTarget);

                foreach (var buffer in _buffersToDispose)
                {
                    buffer.Dispose();
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
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