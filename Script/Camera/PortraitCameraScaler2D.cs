using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortraitCameraScaler2D : MonoBehaviour
{
    public float baseAspect = 9f / 16f; 
    public float baseOrthoSize = 5f;    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        float currentAspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = baseOrthoSize * (baseAspect / currentAspect);
    }
}
