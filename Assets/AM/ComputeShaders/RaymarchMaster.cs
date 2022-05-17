using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RaymarchMaster : MonoBehaviour
{
    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private string featureName = string.Empty;

    private bool TryGetFeature(out ScriptableRendererFeature feature)
    {
        feature = rendererData.rendererFeatures.FirstOrDefault(f => f.name == featureName);
        return feature != null;
        
        // use rendererData.SetDirty(); to tell to renderer to refresh itself
    }
}