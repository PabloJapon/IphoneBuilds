using UnityEngine;
using ZXing;
using ZXing.QrCode;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class QRCodeGenerator : MonoBehaviour
{
    public TMP_Text textFolder;
    public GameObject canvasAbrirCarpeta;
    public Color cornerColor;
    public Sprite cornerMask;
    public GameObject numeroMesa;

    public Texture2D GenerateQRCode(string data, int width, int height, int margin = 1)
    {
        BarcodeWriter barcodeWriter = new BarcodeWriter();
        barcodeWriter.Format = BarcodeFormat.QR_CODE;
        barcodeWriter.Options = new ZXing.Common.EncodingOptions
        {
            Width = width,
            Height = height,
            Margin = margin // Set the margin (quiet zone) here
        };

        Color32[] pixels = barcodeWriter.Write(data);

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels32(pixels);
        texture.Apply();

        // Apply rounded corners to the margin
        int cornerRadius = margin * 10; // Adjust corner radius as needed
        texture = ApplyRoundedCornersToMargin(texture, cornerRadius);

        return texture;
    }

    private Texture2D ApplyRoundedCornersToMargin(Texture2D texture, int cornerRadius)
    {
        int width = texture.width;
        int height = texture.height;
        Texture2D roundedTexture = new Texture2D(width, height);

        // Calculate the corner offset to avoid artifacts at the corner edges
        float cornerOffset = cornerRadius - 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = texture.GetPixel(x, y);

                // Calculate distance from the nearest corner
                float dx = Mathf.Min(x, width - x - 1);
                float dy = Mathf.Min(y, height - y - 1);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Check if the current pixel is within the rounded corner area
                if (dist <= cornerRadius - cornerOffset)
                {
                    // Set the color of pixels within the corner radius
                    pixelColor = cornerColor;
                }

                roundedTexture.SetPixel(x, y, pixelColor);
            }
        }

        roundedTexture.Apply();
        return roundedTexture;
    }

    public void SaveQRCodeImage(Texture2D qrCodeTexture, string filePath)
    {
        byte[] bytes = qrCodeTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
        string digitsMesa = Regex.Replace(filePath, @"\D", "");
        if (digitsMesa == numeroMesa.GetComponent<TMP_Text>().text)
        {
            canvasAbrirCarpeta.SetActive(true);
            textFolder.text = ("Códigos QR guardados en: " + System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\Descargas");
        }
    }

    public string GetSavePath(string fileName)
    {
        // Check if the platform is a mobile device
        if (Application.isMobilePlatform)
        {
            // For mobile, use the Downloads directory
            return System.IO.Path.Combine(Application.persistentDataPath, fileName);
        }
        else
        {
            // For PC, use the Downloads folder
            string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\Downloads";
            return System.IO.Path.Combine(downloadsPath, fileName);
        }
    }
}
