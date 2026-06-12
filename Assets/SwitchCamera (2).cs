using UnityEngine;
using UnityEngine.UI;

public class SwitchCamera : MonoBehaviour
{
    [Tooltip("RawImage UI element to display the camera feed")]
    public RawImage displayImage;

    private WebCamTexture webCamTexture;
    private bool isFrontCamera = false;

    void Start()
    {
        StartCamera();
    }

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying || displayImage == null)
            return;

        // Fix rotation based on device orientation
        displayImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);

        // Mirror horizontally for front camera (natural selfie view)
        displayImage.rectTransform.localScale = isFrontCamera ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

    void OnDestroy()
    {
        StopCamera();
    }

    public void OnCameraChangeMode()
    {
        isFrontCamera = !isFrontCamera;
        StopCamera();
        StartCamera();
        Debug.Log($"Switched to {(isFrontCamera ? "Front" : "Back")} camera");
    }

    private void StartCamera()
    {
        string deviceName = GetCameraName(isFrontCamera);

        if (deviceName == null)
        {
            Debug.LogWarning($"No {(isFrontCamera ? "front" : "back")} camera found.");
            return;
        }

        webCamTexture = new WebCamTexture(deviceName, Screen.width, Screen.height);
        webCamTexture.Play();

        if (displayImage != null)
            displayImage.texture = webCamTexture;
    }

    private void StopCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            Destroy(webCamTexture);
            webCamTexture = null;
        }
    }

    private string GetCameraName(bool wantFront)
    {
        foreach (WebCamDevice device in WebCamTexture.devices)
        {
            if (device.isFrontFacing == wantFront)
                return device.name;
        }
        return null;
    }
}
