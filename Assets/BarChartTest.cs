using UnityEngine;

public class BarChartTest : MonoBehaviour
{
    public float barWidth = 1f; // The width of each bar
    public float barSpacing = 0.5f; // The spacing between bars
    public float barHeight = 1f; // Default height of each bar
    public Color barColor = Color.red; // Public color for all bars

    private float[] originalData; // To store original data values

    public void UpdateBarChart(float[] data, Color[] colors)
    {
        if (data == null || colors == null || data.Length > colors.Length)
        {
            Debug.LogError("Data or colors array is invalid.");
            return;
        }

        // Store the original data
        originalData = (float[])data.Clone();

        // Find the maximum value in the data array
        float maxValue = Mathf.Max(data);
        if (maxValue <= 0)
        {
            Debug.LogError("Max value is zero or negative. Cannot normalize.");
            return;
        }

        // Clear old bars
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Create bars
        for (int i = 0; i < data.Length; i++)
        {
            // Normalize data[i] by maxValue
            float normalizedHeight = (data[i] / maxValue) * barHeight;
            CreateBar(i, normalizedHeight, data[i]);
        }
    }

    private void CreateBar(int index, float height, float originalValue)
    {
        // Create the bar
        GameObject bar = new GameObject("Bar" + index);
        bar.transform.SetParent(transform);

        MeshFilter meshFilter = bar.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = bar.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Create vertices for the bar
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0, 0, 0); // Bottom-left
        vertices[1] = new Vector3(barWidth, 0, 0); // Bottom-right
        vertices[2] = new Vector3(0, height, 0); // Top-left
        vertices[3] = new Vector3(barWidth, height, 0); // Top-right

        // Define the triangles for the bar
        int[] triangles = new int[]
        {
        0, 2, 1, // First triangle
        2, 3, 1  // Second triangle
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Create and set the material with the public barColor
        Material barMaterial = new Material(Shader.Find("UI/Default"));
        barMaterial.color = barColor; // Set the color of the bar
        meshRenderer.material = barMaterial;

        // Position the bar
        bar.transform.localPosition = new Vector3(index * (barWidth + barSpacing), 0, 0);
        bar.transform.rotation = Quaternion.Euler(-180f, 0f, 0f); // Rotate 180 degrees around the X axis

        // Create the label
        GameObject labelObject = new GameObject("Label" + index);
        labelObject.transform.SetParent(bar.transform);
        labelObject.transform.localPosition = new Vector3(barWidth / 2, height + 0.1f, 0); // Position above the bar

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = originalValue.ToString("F0"); // Set the text to the original value with one decimal place
        textMesh.color = Color.black; // Text color
        textMesh.anchor = TextAnchor.MiddleCenter; // Center the text
        textMesh.alignment = TextAlignment.Center;

        // Optionally adjust font size and other properties
        textMesh.fontSize = 14;
        textMesh.characterSize = 0.1f;

        // Ensure the label has no rotation
        labelObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Correct for the bar's rotation
    }
}
