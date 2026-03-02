using UnityEngine;
using System.IO;
using TMPro;
using System.Collections;

public class PhotoCaptureBothCams : MonoBehaviour
{
    [Header("Cameras")]
    public Camera thirdPersonCam;
    public Camera noseCam;

    [Header("Capture Settings")]
    public KeyCode captureKey = KeyCode.P;
    public int width = 1920;
    public int height = 1080;

    [Header("UI")]
    public TMP_Text text;

    void Start()
    {
        if (text != null)
            text.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            CaptureFromActiveCamera();
        }
    }

    void CaptureFromActiveCamera()
    {
        Camera activeCam = noseCam != null && noseCam.enabled ? noseCam : thirdPersonCam;
        if (activeCam == null)
        {
            Debug.LogError("PHOTO CAPTURE FAILED: No active camera");
            return;
        }

        RenderTexture rt = new RenderTexture(width, height, 24);
        activeCam.targetTexture = rt;

        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        activeCam.Render();

        RenderTexture.active = rt;
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        activeCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = image.EncodeToPNG();

        // 🔑 SAVE NEXT TO EXE / APP
        string rootPath = Path.GetDirectoryName(Application.dataPath);
        string folderPath = Path.Combine(rootPath, "CapturedPhotos");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = Path.Combine(
            folderPath,
            "Photo_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".png"
        );

        File.WriteAllBytes(fileName, bytes);

        Debug.Log("PHOTO SAVED AT: " + fileName);

        if (text != null)
        {
            text.text = "Photo saved:\n" + fileName;
            text.enabled = true;
            StartCoroutine(HideText());
        }
    }

    IEnumerator HideText()
    {
        yield return new WaitForSeconds(5f);
        if (text != null)
            text.enabled = false;
    }
}