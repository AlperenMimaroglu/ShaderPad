using AM.ComputeShaders.Pass;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RaymarchFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRendering;
        public ComputeShader raymarchShader;
        public int maxSteps;
        public int maxDistance;
        public float surfDistance;
    }

    private RayMarchPass _pass;
    public PassSettings passSettings = new();

    public override void Create()
    {
        _pass = new RayMarchPass(passSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}