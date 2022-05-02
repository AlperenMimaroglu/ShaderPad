using UnityEngine;

public class PointToPlaneExample : MonoBehaviour
{
    [SerializeField] private GameObject plane;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    [ContextMenu("Refresh")]
    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        var planeVector = plane.transform.position;

        var material = _renderer.material;
        material.SetVector("_PlaneNormal", plane.transform.up);
        material.SetVector("_PlanePos", planeVector);
    }
}