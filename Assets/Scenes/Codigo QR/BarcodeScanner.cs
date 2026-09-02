using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using TMPro;
using System;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class BarcodeScanner : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private BarcodeReader barcodeReader;
    public RawImage rawImage;
    public TMP_Text text;
    public GameObject navigation;

    private float scanInterval = 0.5f; // Scan every 0.5 second
    private float timeSinceLastScan = 0.0f;

    private bool uvRectCalculado = false;
    private Rect uvRectCache;

    void Start()
    {
        // Check camera permission at the start
        if (CheckCameraPermission())
        {
            InitializeWebCam();
        }
        else
        {
            RequestCameraPermission();
        }
    }

    // Check if camera permission is granted
    bool CheckCameraPermission()
    {
#if UNITY_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
        return true; // iOS automatically handles permission when requested
#endif
    }

    // Request camera permission
    void RequestCameraPermission()
    {
    #if UNITY_ANDROID
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += OnCameraPermissionGranted;
        callbacks.PermissionDenied += OnCameraPermissionDenied;
        callbacks.PermissionDeniedAndDontAskAgain += OnCameraPermissionDenied;

        Permission.RequestUserPermission(Permission.Camera, callbacks);
    #endif
    }

    #if UNITY_ANDROID
    private void OnCameraPermissionGranted(string permissionName)
    {
        InitializeWebCam();
    }

    private void OnCameraPermissionDenied(string permissionName)
    {
        Debug.LogWarning("Permiso de cámara denegado: " + permissionName);
        if (text != null) text.text = "Necesitamos acceso a la cámara para escanear el QR.";
    }
    #endif

    // Initialize webcam once permission is granted
    void InitializeWebCam()
    {
        barcodeReader = new BarcodeReader();
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();

        if (rawImage != null)
        {
            rawImage.texture = webcamTexture;
        }
    }

    // Converts a scanned URL like "https://gastrali.com/qr?m=5&r=123"
    // or "https://gastrali.com/qr?camarero=1&r=123" back into the
    // legacy "5;123" / "Camarero;123" format the app already expects.
    // If the scanned text is NOT a recognized URL, it's returned unchanged
    // (keeps backward compatibility with old printed QR codes).
    private string ParseScannedText(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        if (raw.StartsWith("http://") || raw.StartsWith("https://"))
        {
            try
            {
                Uri uri = new Uri(raw);
                var query = ParseQueryStringSimple(uri.Query);

                string mesa = query.TryGetValue("m", out var m) ? m : null;
                string camarero = query.TryGetValue("camarero", out var c) ? c : null;
                string restaurantId = query.TryGetValue("r", out var r) ? r : null;

                if (!string.IsNullOrEmpty(restaurantId))
                {
                    if (!string.IsNullOrEmpty(mesa))
                        return $"{mesa};{restaurantId}";

                    if (!string.IsNullOrEmpty(camarero))
                        return $"Camarero;{restaurantId}";
                }
            }
            catch
            {
                // Malformed URL — fall through and return raw text below
            }
        }

        // Not a URL, or parsing failed — treat as already in legacy format
        return raw;
    }

    private System.Collections.Generic.Dictionary<string, string> ParseQueryStringSimple(string query)
    {
        var result = new System.Collections.Generic.Dictionary<string, string>();
        if (string.IsNullOrEmpty(query)) return result;

        if (query.StartsWith("?")) query = query.Substring(1);

        foreach (var pair in query.Split('&'))
        {
            if (string.IsNullOrEmpty(pair)) continue;
            var parts = pair.Split(new[] { '=' }, 2);
            string key = Uri.UnescapeDataString(parts[0]);
            string value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
            result[key] = value;
        }
        return result;
    }

    private void Update()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            rawImage.material.mainTexture = webcamTexture;

            if (!uvRectCalculado && webcamTexture.width > 100) // width>100 evita frames iniciales inválidos
            {
                CalcularUvRectCover();
                uvRectCalculado = true;
            }

            rawImage.uvRect = uvRectCache;

            // Check if it's time to scan again
            timeSinceLastScan += Time.deltaTime;
            if (timeSinceLastScan >= scanInterval)
            {
                StartScanning();
                timeSinceLastScan = 0.0f; // Reset the timer
            }
        }
    }

    private void CalcularUvRectCover()
    {
        float rectW = rawImage.rectTransform.rect.width;
        float rectH = rawImage.rectTransform.rect.height;
        float targetAspect = rectW / rectH;
        float camAspect = (float)webcamTexture.width / webcamTexture.height;

        float uOffset = 0f, uWidth = 1f, vOffset = 0f, vHeight = 1f;

        if (camAspect > targetAspect)
        {
            // La cámara es más "ancha" que el contenedor -> recortamos laterales
            uWidth = targetAspect / camAspect;
            uOffset = (1f - uWidth) / 2f;
        }
        else if (camAspect < targetAspect)
        {
            // La cámara es más "alta" que el contenedor -> recortamos arriba/abajo
            vHeight = camAspect / targetAspect;
            vOffset = (1f - vHeight) / 2f;
        }

        // Se aplica junto con el flip horizontal que ya tenías (mirror de la cámara frontal/trasera)
        uvRectCache = new Rect(uOffset + uWidth, vOffset, -uWidth, vHeight);
    }

    // Trigger the barcode scanner
    public void StartScanning()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            // Read the barcode from the current camera frame
            Result result = barcodeReader.Decode(webcamTexture.GetPixels32(), webcamTexture.width, webcamTexture.height);
            text.text = ("Scanned Barcode: " + "None yet");

            if (result != null)
            {
                text.text = ("Scanned Barcode: " + result.Text);
                if (navigation != null)
                {
                    string parsedResult = ParseScannedText(result.Text);
                    // Call the ProcessQRCodeResult method using SendMessage
                    navigation.SendMessage("ProcessQRCodeResult", parsedResult);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
}
