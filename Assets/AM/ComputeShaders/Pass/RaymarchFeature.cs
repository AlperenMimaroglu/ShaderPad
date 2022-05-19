using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AM.ComputeShaders
{
    public class RaymarchFeature : ScriptableRendererFeature
    {
        private bool _initialized = false;

        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRendering;
            public ComputeShader raymarchShader;
            public ShapeData[] shapeData = Array.Empty<ShapeData>();
            public string kernelName = "RaymarchFeature";
        }

        private RaymarchPass _pass;
        public Settings settings = new();

        public void Init(ShapeData[] shapeData)
        {
            _initialized = true;

            settings.shapeData = shapeData;
            Create();
        }

        public override void Create()
        {
            if (!_initialized) return;

            _pass = new RaymarchPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_initialized)
            {
                renderer.EnqueuePass(_pass);
            }
        }
    }
}