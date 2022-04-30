using UnityEngine;

public class PointToPlaneExample : MonoBehaviour
{
    [SerializeField] private GameObject plane;

    [ContextMenu("Refresh")]
    private void Refresh()
    {
        var planeVector = plane.transform.position;
        var h = Vector3.Dot(plane.transform.up, planeVector);

        Debug.Log(h);

        var renderer = GetComponent<Renderer>();
        var material = renderer.material;
        material.SetVector("_Normal", plane.transform.up);
        material.SetFloat("_Height", h);
        material.SetVector("_PlanePos", planeVector);
    }
}