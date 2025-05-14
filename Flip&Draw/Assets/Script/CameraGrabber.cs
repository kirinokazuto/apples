using UnityEngine;


public class CameraGrabber : MonoBehaviour
{
    private void Awake()
    {
        if (TryGetComponent(out Canvas canvas))
            canvas.worldCamera = Camera.main;
    }
}
