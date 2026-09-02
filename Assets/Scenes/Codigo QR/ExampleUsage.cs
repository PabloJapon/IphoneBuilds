using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using UnityEngine.UI;

public class ExampleUsage : MonoBehaviour
{
    public QRCodeGenerator qrCodeGenerator;
    public TMP_Text contentNumeroMesas;
    public TMP_Text mesaText;
    public Texture2D logoTexture;
    public string saveFolderName = "Qr Mesas";
    public string saveFileNamePrefix = "Qr Mesa ";
    public TMP_Dropdown formatDropdown;

    public string idRestaurante;
    private int qrCodeMargin = 1;

    public RectTransform uiCaptureElement; // UI element to capture

    public void CreateQRAndSave()
    {
        StartCoroutine(CaptureScreenAndSave());
    }

    private System.Collections.IEnumerator CaptureScreenAndSave()
    {
        yield return new WaitForEndOfFrame();

        string downloadsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
        string folderPath = Path.Combine(downloadsPath, saveFolderName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string selectedContent = contentNumeroMesas.text;
        int tableNumber;

        List<string> savedImagePaths = new List<string>();

        // Generate QR for Mesa 0 (Waiter)
        mesaText.text = "Personal";
        yield return new WaitForEndOfFrame();
        string waiterImagePath = SaveQRCodeForTable("0", folderPath);
        savedImagePaths.Add(waiterImagePath);

        if (int.TryParse(selectedContent, out tableNumber))
        {
            for (int i = 1; i <= tableNumber; i++)
            {
                mesaText.text = "Mesa " + i;
                yield return new WaitForEndOfFrame();
                string imagePath = SaveQRCodeForTable(i.ToString(), folderPath);
                savedImagePaths.Add(imagePath);
            }
        }
        else
        {
            mesaText.text = "Mesa " + selectedContent;
            yield return new WaitForEndOfFrame();
            string imagePath = SaveQRCodeForTable(selectedContent, folderPath);
            savedImagePaths.Add(imagePath);
        }

        mesaText.text = "Mesa 1";

        if (formatDropdown.value == 1)
        {
            string pdfPath = Path.Combine(folderPath, "All_QR_Codes.pdf");
            SaveAllQRCodesToPDF(savedImagePaths, pdfPath);

            // Optional: delete PNGs after creating the PDF
            foreach (string path in savedImagePaths)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }

    private string SaveQRCodeForTable(string tableContent, string folderPath)
    {
        string saveFileName = (tableContent == "0") ? "Personal.png" : saveFileNamePrefix + tableContent + ".png";
        string imagePath = Path.Combine(folderPath, saveFileName);

        Texture2D screenTexture = CaptureUIElement();
        if (screenTexture == null) return imagePath;

        EmbedQRCodeAndLogo(tableContent, screenTexture);

        qrCodeGenerator.SaveQRCodeImage(screenTexture, imagePath);

        return imagePath;
    }

    Texture2D CaptureUIElement()
    {
        if (uiCaptureElement == null)
        {
            Debug.LogError("UI Capture Element is not assigned!");
            return null;
        }

        Vector3[] worldCorners = new Vector3[4];
        uiCaptureElement.GetWorldCorners(worldCorners);

        float x = worldCorners[0].x;
        float y = worldCorners[0].y;
        float width = worldCorners[2].x - worldCorners[0].x;
        float height = worldCorners[2].y - worldCorners[0].y;

        // Flip Y coordinate because ReadPixels starts from bottom-left
        Rect captureRect = new Rect(x, y, width, height);
        Texture2D screenTexture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        screenTexture.ReadPixels(captureRect, 0, 0);
        screenTexture.Apply();
        return screenTexture;
    }

    void EmbedQRCodeAndLogo(string data, Texture2D screenTexture)
    {
        idRestaurante = LoginManagerResponsable.restaurantID;
        string qrCodeData = (data == "0") ? "Camarero;" + idRestaurante : data + ";" + idRestaurante;

        Texture2D qrCodeTexture = qrCodeGenerator.GenerateQRCode(qrCodeData, 256, 256, qrCodeMargin);

        int qrCodePosX = (screenTexture.width - qrCodeTexture.width) / 2;
        int qrCodePosY = (screenTexture.height - qrCodeTexture.height) / 2;

        for (int y = 0; y < qrCodeTexture.height; y++)
        {
            for (int x = 0; x < qrCodeTexture.width; x++)
            {
                Color qrColor = qrCodeTexture.GetPixel(x, y);
                screenTexture.SetPixel(qrCodePosX + x, qrCodePosY + y, qrColor);
            }
        }

        screenTexture.Apply();
    }

    void SaveAllQRCodesToPDF(List<string> imagePaths, string pdfPath)
    {
        Document document = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
        PdfWriter.GetInstance(document, new FileStream(pdfPath, FileMode.Create));

        document.Open();
        int imagesPerRow = 4;
        float qrCodeSize = (PageSize.A4.Width - 50f) / imagesPerRow;
        PdfPTable table = new PdfPTable(imagesPerRow);
        table.WidthPercentage = 100;

        foreach (string imagePath in imagePaths)
        {
            if (File.Exists(imagePath))
            {
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                img.ScaleToFit(qrCodeSize, qrCodeSize);
                PdfPCell cell = new PdfPCell(img, true)
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Border = Rectangle.NO_BORDER,
                    Padding = 5
                };
                table.AddCell(cell);
            }
        }

        // Pad to complete the last row
        int remainder = imagePaths.Count % imagesPerRow;
        if (remainder != 0)
        {
            for (int i = 0; i < imagesPerRow - remainder; i++)
            {
                PdfPCell emptyCell = new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER };
                table.AddCell(emptyCell);
            }
        }

        document.Add(table);
        document.Close();
    }
}
