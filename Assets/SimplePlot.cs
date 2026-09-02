using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimplePlot : MonoBehaviour
{
    [Header("UI Reference")]
    public RawImage plotImage; // Assign a RawImage component from your Canvas

    [Header("Texture Settings")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color backgroundColor = Color.white;

    [Header("Axis Settings")]
    public int margin = 40; // Margin for axes drawing
    public Color axisColor = Color.black;
    public List<string> xLabels = new List<string>(); // Store labels for X-axis
    public GameObject xLabelPrefab; // Prefab for X-axis text labels
    public Transform xLabelParent; // Parent object for X-axis labels

    // Y-axis label and grid fields:
    public GameObject yLabelPrefab; // Prefab for Y-axis text labels
    public Transform yLabelParent; // Parent object for Y-axis labels
    public int yAxisTickCount = 5; // Number of tick intervals (labels and grid lines drawn below the top border)
    public Color gridColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Light gray for grid lines

    [Header("Line Settings")]
    public Color lineColor = Color.blue;
    public int lineThickness = 3; // Thickness for all drawn lines

    [Header("Smoothing Options")]
    public bool smoothLines = false; // Enable smoothing between data points
    public int smoothingSubdivisions = 10; // Number of points sampled per segment

    [Header("Data")]
    [HideInInspector] public float[] xData; // X-axis data (e.g., days)
    [HideInInspector] public float[] yData; // Y-axis data (e.g., earnings)

    // Data range (set these via code before drawing)
    public float xMin = 0f;
    public float xMax = 31f; // For example, for a month
    public float yMin = 0f;
    public float yMax = 100f;

    /// <summary>
    /// Sets the X-axis labels.
    /// </summary>
    public void SetXLabels(List<string> labels)
    {
        xLabels = labels;
    }

    /// <summary>
    /// Main entry point to draw the plot.
    /// </summary>
    public void DrawPlot()
    {
        // Clear previous X-axis labels.
        foreach (Transform child in xLabelParent)
        {
            Destroy(child.gameObject);
        }
        // Clear previous Y-axis labels.
        foreach (Transform child in yLabelParent)
        {
            Destroy(child.gameObject);
        }

        // Validate data arrays.
        if (xData == null || yData == null || xData.Length != yData.Length)
        {
            Debug.LogError("Invalid data arrays for plot. xData Length: " 
                + (xData != null ? xData.Length.ToString() : "NULL") 
                + ", yData Length: " + (yData != null ? yData.Length.ToString() : "NULL"));
            return;
        }

        if (xLabels == null || xLabels.Count == 0)
        {
            Debug.LogError("No X labels provided!");
            return;
        }

        // Create and fill the texture with the background color.
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        Color[] bg = new Color[textureWidth * textureHeight];
        for (int i = 0; i < bg.Length; i++)
            bg[i] = backgroundColor;
        tex.SetPixels(bg);

        // --- Compute a "nice" maximum for the Y axis and tick spacing.
        // We assume yMin is zero (if not, this can be adjusted).
        float niceMax = RoundToNiceNumberUp(yMax);
        float tickStep = niceMax / yAxisTickCount;

        // --- Draw horizontal grid lines (behind the plot) and generate Y-axis labels.
        // We draw ticks for i=0 to yAxisTickCount-1; the top border (i==yAxisTickCount) is drawn as the Y-axis.
        for (int i = 0; i < yAxisTickCount; i++)
        {
            float tickValue = i * tickStep;
            int yPos = margin + Mathf.RoundToInt((tickValue - yMin) / (niceMax - yMin) * (textureHeight - 2 * margin));
            DrawLine(tex, margin, yPos, textureWidth - margin, yPos, gridColor);

            // Instantiate label for this tick.
            GameObject yLabel = Instantiate(yLabelPrefab, yLabelParent);
            yLabel.GetComponent<TextMeshProUGUI>().text = tickValue.ToString("F0");
            RectTransform rectTransform = yLabel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(margin - 30, yPos);
        }

        // --- Map data points to texture coordinates.
        Vector2[] dataPoints = new Vector2[xData.Length];
        for (int i = 0; i < xData.Length; i++)
        {
            float normX = Mathf.InverseLerp(xMin, xMax, xData[i]);
            // Use the computed niceMax for vertical mapping so the grid and labels align.
            float normY = Mathf.InverseLerp(yMin, niceMax, yData[i]);
            int px = margin + Mathf.RoundToInt(normX * (textureWidth - 2 * margin));
            int py = margin + Mathf.RoundToInt(normY * (textureHeight - 2 * margin));
            dataPoints[i] = new Vector2(px, py);
        }

        // --- Draw the line plot.
        if (smoothLines && dataPoints.Length >= 2)
        {
            List<Vector2> smoothPoints = GetBezierSmoothedPoints(dataPoints, smoothingSubdivisions);
            for (int i = 0; i < smoothPoints.Count - 1; i++)
            {
                DrawLine(tex, (int)smoothPoints[i].x, (int)smoothPoints[i].y, (int)smoothPoints[i + 1].x, (int)smoothPoints[i + 1].y, lineColor);
            }
        }
        else
        {
            for (int i = 0; i < dataPoints.Length - 1; i++)
            {
                DrawLine(tex, (int)dataPoints[i].x, (int)dataPoints[i].y, (int)dataPoints[i + 1].x, (int)dataPoints[i + 1].y, lineColor);
            }
        }

        // --- Draw the axes on top of the line plot ---
        DrawLine(tex, margin, margin, textureWidth - margin, margin, axisColor);    // X-axis
        //DrawLine(tex, margin, margin, margin, textureHeight - margin, axisColor);   // Y-axis

        // --- Generate X-axis labels.
        for (int i = 0; i < xLabels.Count; i++)
        {
            // Avoid index out-of-range if there are more labels than data points.
            if (i >= xData.Length)
            {
                Debug.LogError($"Skipping label {i} because it's out of bounds! xLabels.Count={xLabels.Count}, xData.Length={xData.Length}");
                continue;
            }

            GameObject label = Instantiate(xLabelPrefab, xLabelParent);
            label.GetComponent<TextMeshProUGUI>().text = xLabels[i];
            float normX = Mathf.InverseLerp(xMin, xMax, xData[i]);
            float xPos = margin + normX * (textureWidth - 2 * margin);
            RectTransform rectTransform = label.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(xPos, -30);
        }

        tex.Apply();
        plotImage.texture = tex;
    }

    /// <summary>
    /// Draws a line on the texture using a modified Bresenham algorithm that accounts for line thickness.
    /// </summary>
    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            DrawThickPixel(tex, x0, y0, col);
            if (x0 == x1 && y0 == y1)
                break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    /// <summary>
    /// Draws a pixel and its neighbors to simulate a thicker pixel.
    /// </summary>
    void DrawThickPixel(Texture2D tex, int x, int y, Color col)
    {
        int half = lineThickness / 2;
        for (int ix = -half; ix <= half; ix++)
        {
            for (int iy = -half; iy <= half; iy++)
            {
                int drawX = x + ix;
                int drawY = y + iy;
                if (drawX >= 0 && drawX < tex.width && drawY >= 0 && drawY < tex.height)
                {
                    tex.SetPixel(drawX, drawY, col);
                }
            }
        }
    }

    /// <summary>
    /// Uses a cubic Bézier approach to smooth the data points.
    /// For each segment between two consecutive data points, control points are computed
    /// based on the neighboring points (with linear extrapolation at the endpoints).
    /// </summary>
    List<Vector2> GetBezierSmoothedPoints(Vector2[] points, int subdivisions)
    {
        List<Vector2> smoothPoints = new List<Vector2>();

        if (points.Length < 2)
        {
            smoothPoints.AddRange(points);
            return smoothPoints;
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 p0 = i == 0 ? points[i] : points[i - 1];
            Vector2 p1 = points[i];
            Vector2 p2 = points[i + 1];
            Vector2 p3 = (i + 2 < points.Length) ? points[i + 2] : points[i + 1];

            Vector2 c1 = p1 + (p2 - p0) / 6f;
            Vector2 c2 = p2 - (p3 - p1) / 6f;

            if (i == 0)
            {
                smoothPoints.Add(p1);
            }

            for (int j = 1; j <= subdivisions; j++)
            {
                float t = j / (float)(subdivisions + 1);
                Vector2 bezierPoint = CalculateCubicBezierPoint(t, p1, c1, c2, p2);
                smoothPoints.Add(bezierPoint);
            }
            smoothPoints.Add(p2);
        }
        return smoothPoints;
    }

    /// <summary>
    /// Calculates a point on a cubic Bézier curve given parameter t in [0,1].
    /// </summary>
    Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 c1, Vector2 c2, Vector2 p1)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 point = uuu * p0;
        point += 3 * uu * t * c1;
        point += 3 * u * tt * c2;
        point += ttt * p1;
        return point;
    }

    /// <summary>
    /// Rounds the given value up to a "nice" number.
    /// For example, 1462 becomes 1500 and 238 becomes 250.
    /// </summary>
    float RoundToNiceNumberUp(float value)
    {
        if (value == 0)
            return 0;

        float exponent = Mathf.Floor(Mathf.Log10(value));
        float fraction = value / Mathf.Pow(10, exponent);
        float niceFraction = 1f;

        if (fraction <= 1)
            niceFraction = 1;
        else if (fraction <= 2)
            niceFraction = 2;
        else if (fraction <= 5)
            niceFraction = 5;
        else
            niceFraction = 10;

        return niceFraction * Mathf.Pow(10, exponent);
    }
}
