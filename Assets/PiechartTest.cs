using UnityEngine;

public class PiechartTest : MonoBehaviour
{
    public void UpdatePieChart(float[] data, Color[] colors)
    {
        // Clear previous slices
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        if (data == null || colors == null || data.Length > colors.Length)
        {
            Debug.LogError("Data or colors array is invalid.");
            return;
        }

        float currentAngle = 0f;

        for (int i = 0; i < data.Length; i++)
        {
            float sliceAngle = data[i] / 100f * 360f;
            CreateSlice(currentAngle, sliceAngle, colors[i]);
            currentAngle += sliceAngle;
        }
    }

    private void CreateSlice(float startAngle, float sliceAngle, Color color)
    {
        GameObject slice = new GameObject("Slice");
        slice.transform.SetParent(transform);

        MeshFilter meshFilter = slice.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = slice.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        int segments = 500; // Set number of segments for smoothness
        Vector3[] vertices = new Vector3[segments + 2]; // We add 2 instead of 1 to ensure proper vertex count
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // The central vertex at (0,0,0)

        // Generate vertices around the circle
        for (int i = 0; i <= segments; i++) // <= to cover the full range including the last point
        {
            float angle = startAngle + (i / (float)segments) * sliceAngle;
            float rad = Mathf.Deg2Rad * angle;
            float x = Mathf.Cos(rad);
            float y = Mathf.Sin(rad);

            vertices[i + 1] = new Vector3(x, y, 0);
        }

        // Generate triangles for the slice
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // Center vertex
            triangles[i * 3 + 1] = i + 1; // Current vertex
            triangles[i * 3 + 2] = i + 2; // Next vertex (next index in vertices array)
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = color };
    }
}
