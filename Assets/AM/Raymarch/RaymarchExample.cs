using UnityEngine;

public class RaymarchExample : MonoBehaviour
{
    [SerializeField] private GameObject raymarchVolume;
    [SerializeField] private Transform cubePos;
    [SerializeField] private Transform SpherePos;

    private Material raymarcher;

    private void Awake()
    {
        raymarcher = raymarchVolume.GetComponent<Renderer>().material;
    }

    private void Update()
    {
        raymarcher.SetVector("_Cube",
            new Vector4(cubePos.transform.position.x, cubePos.transform.position.y, cubePos.transform.position.z, 1));
        raymarcher.SetVector("_Sphere",
            new Vector4(SpherePos.transform.position.x, SpherePos.transform.position.y, SpherePos.transform.position.z,
                1));
    }
}