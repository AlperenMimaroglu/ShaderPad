using UnityEngine;

public class USBSimpleColorController : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture mainTex;
    private int texSize = 256;
    private Renderer rend;

    private void Start()
    {
        mainTex = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.ARGB32);
        mainTex.enableRandomWrite = true;
        mainTex.Create();

        rend = GetComponent<Renderer>();

        rend.enabled = true;

        computeShader.SetTexture(0, "Result", mainTex);
        rend.material.SetTexture("_BaseMap", mainTex);

        computeShader.Dispatch(0, texSize / 8, texSize / 8, 1);
    }
}