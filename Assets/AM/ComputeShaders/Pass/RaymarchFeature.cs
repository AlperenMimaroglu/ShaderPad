using AM.ComputeShaders.Pass;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RaymarchFeature : ScriptableRendererFeature
{
    private bool _initialized;

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRendering;
        public ComputeShader raymarchShader;

        public string kernelName = "Dummy";
        public int maxSteps = 100;
        public int maxDistance = 100;
        public float surfDistance = .001f;
    }

    private RaymarchPass _pass;
    public Settings settings = new();

    public override void Create()
    {
        if (settings.raymarchShader == null)
        {
            _initialized = false;
            return;
        }

        _pass = new RaymarchPass(settings);
        _initialized = true;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_initialized)
        {
            renderer.EnqueuePass(_pass);
        }
    }
}