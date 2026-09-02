using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Renamed enum to DataFilterMode to avoid conflicts with other assets
public enum DataFilterMode
{
    Lifetime,      // No filter, show all data
    CurrentMonth,  // Only data from the current month
    LastMonth,     // Only data from the previous month
    Last6Months,   // Data from the last 6 months (up to now)
    Last12Months   // Data from the last 12 months (up to now)
}

public class CrearRegistrosPedidos : MonoBehaviour
{
    public PiechartTest piechartTest;  // Reference to the PiechartTest script
    public BarChartTest barChartTest;
    public SimplePlot simplePlot;
    public GameObject legendPanel;     // Reference to the UI panel for the legend
    public GameObject legendItemPrefab; // Prefab for each legend item (with color box and label)
    public TMP_Text totalGananciasText; // Text component to display total "Ganancias"
    public TMP_Text totalPedidosText;   // Text component to display total number of "Pedidos"
    public GameObject noHayDatosParaEsteFiltro; 

    // Panels
    public GameObject topPlatosPanel;
    public GameObject topPlatoItemPrefab;

    public GameObject topMesasPanel;
    public GameObject topMesaItemPrefab;

    // Dropdowns
    public TMP_Dropdown sortingPlatosDropdown;
    public TMP_Dropdown sortingMesasDropdown;

    // Class-level filtered arrays for use in dropdown callbacks
    private string[] filteredPlato;
    private string[] filteredN;
    private string[] filteredPreciosPlatos;
    private int[] filteredMesa;

    private DataFilterMode currentFilter = DataFilterMode.CurrentMonth;

    void Start()
    {
        // Set the correct event listeners for each dropdown
        sortingPlatosDropdown.onValueChanged.AddListener(OnPlatosDropdownValueChanged);
        sortingMesasDropdown.onValueChanged.AddListener(OnMesasDropdownValueChanged);
        StartCoroutine(WaitForDataLoad());
    }

    public void OnPlatosDropdownValueChanged(int index)
    {
        // Use the stored filtered arrays and the sort index from the dropdown
        DisplayTopPlatos(filteredPlato, filteredN, filteredPreciosPlatos, index);
    }
    public void OnMesasDropdownValueChanged(int index)
    {
        DisplayTopMesas(filteredMesa, filteredN, filteredPreciosPlatos, index);
    }

    IEnumerator WaitForDataLoad()
    {
        while (!DataBaseRegistros.isDataLoaded)
        {
            yield return null; // Wait until data is loaded
        }

        // Get the indices for data that passes the filter
        List<int> indices = GetFilteredIndices(currentFilter);

        // Create filtered versions of your arrays
        if (indices.Count == 0)
        {
            //Debug.Log("No hay datos para este filtro");
            noHayDatosParaEsteFiltro.SetActive(true);
            yield break;
        }
        else
        {
            noHayDatosParaEsteFiltro.SetActive(false);
        }

        string[] filteredFechas = indices.Select(i => DataBaseRegistros.fecha[i]).ToArray();
        string[] filteredCategorias = indices
            .SelectMany(i => DataBaseRegistros.categoria[i]
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        string[] filteredNLocal = indices
            .SelectMany(i => DataBaseRegistros.n[i]
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        string[] filteredPreciosPlatosLocal = indices
            .SelectMany(i => DataBaseRegistros.precioPlato[i]
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        string[] filteredPlato = indices
            .SelectMany(i => DataBaseRegistros.plato[i]
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        filteredMesa = indices.Select(i => DataBaseRegistros.mesa[i]).ToArray();

        // Also assign to the class-level variables for calculations
        filteredN = filteredNLocal;
        filteredPreciosPlatos = filteredPreciosPlatosLocal;

        // Now call your methods with the filtered arrays:
        GenerateChart(filteredCategorias, filteredNLocal);
        GenerateBarChart(filteredFechas, filteredNLocal);
        GeneratePlot(filteredFechas, filteredNLocal, filteredPreciosPlatosLocal, currentFilter);
        CalculateTotalGanancias(filteredPreciosPlatosLocal, filteredNLocal);
        CalculateTotalPedidos(filteredNLocal);
        // Display initial top dishes and tables with default sort index (0)
        DisplayTopPlatos(filteredPlato, filteredNLocal, filteredPreciosPlatosLocal, 0);
        DisplayTopMesas(filteredMesa, filteredNLocal, filteredPreciosPlatosLocal, 0);
    }

    private List<int> GetFilteredIndices(DataFilterMode mode)
    {
        List<int> indices = new List<int>();
        DateTime now = DateTime.Now;

        for (int i = 0; i < DataBaseRegistros.fecha.Length; i++)
        {
            DateTime date;
            if (!DateTime.TryParseExact(
                    DataBaseRegistros.fecha[i],
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out date))
            {
                Debug.LogWarning($"Invalid date format: {DataBaseRegistros.fecha[i]}");
                continue;
            }

            switch (mode)
            {
                case DataFilterMode.Lifetime:
                    currentFilter = DataFilterMode.Lifetime;
                    // Include all data
                    indices.Add(i);
                    break;
                case DataFilterMode.CurrentMonth:
                    currentFilter = DataFilterMode.CurrentMonth;
                    if (date.Year == now.Year && date.Month == now.Month)
                        indices.Add(i);
                    break;
                case DataFilterMode.LastMonth:
                    currentFilter = DataFilterMode.LastMonth;
                    DateTime lastMonth = now.AddMonths(-1);
                    if (date.Year == lastMonth.Year && date.Month == lastMonth.Month)
                        indices.Add(i);
                    break;
                case DataFilterMode.Last6Months:
                    currentFilter = DataFilterMode.Last6Months;
                    // Include data from now minus 6 months up to now
                    if (date >= now.AddMonths(-6) && date <= now)
                        indices.Add(i);
                    break;
                case DataFilterMode.Last12Months:
                    currentFilter = DataFilterMode.Last12Months;
                    // Include data from now minus 12 months up to now
                    if (date >= now.AddMonths(-12) && date <= now)
                        indices.Add(i);
                    break;
            }
        }

        return indices;
    }

    private void GenerateChart(string[] categorias, string[] n)
    {
        // Clear previous legend items
        foreach (Transform child in legendPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (categorias == null || categorias.Length == 0)
        {
            Debug.LogWarning("No data available.");
            return;
        }

        Dictionary<string, int> categorySums = new Dictionary<string, int>();
        int totalN = 0;

        // Sum data for each category
        for (int i = 0; i < categorias.Length; i++)
        {
            if (!categorySums.ContainsKey(categorias[i]))
                categorySums[categorias[i]] = 0;

            categorySums[categorias[i]] += int.Parse(n[i]);
            totalN += int.Parse(n[i]);
        }

        // Predefined colors
        Color[] predefinedColors = new Color[]
        {
            new Color(0.94f, 0.76f, 0.20f),  // Soft Yellow
            new Color(0.90f, 0.30f, 0.24f),  // Coral Red
            new Color(0.17f, 0.60f, 0.90f),  // Sky Blue
            new Color(0.48f, 0.78f, 0.64f),  // Mint Green
            new Color(0.87f, 0.44f, 0.65f),  // Soft Pink
            new Color(0.35f, 0.70f, 0.90f),  // Light Blue
            new Color(0.60f, 0.40f, 0.88f),  // Soft Purple
            new Color(0.98f, 0.68f, 0.30f),  // Orange
            new Color(0.30f, 0.75f, 0.55f)   // Greenish-Blue
        };

        List<float> percentages = new List<float>();
        List<Color> colors = new List<Color>();

        int index = 0;
        foreach (var category in categorySums)
        {
            float percentage = (float)category.Value / totalN * 100f;
            percentages.Add(percentage);

            Color color = predefinedColors[index % predefinedColors.Length];
            colors.Add(color);

            CreateLegendItem(category.Key, color, percentage);
            index++;
        }

        // Update your pie chart
        piechartTest.UpdatePieChart(percentages.ToArray(), colors.ToArray());
    }

    private void CreateLegendItem(string categoryName, Color color, float percentage)
    {
        // Instantiate the legend item prefab
        GameObject legendItem = Instantiate(legendItemPrefab, legendPanel.transform);

        // Set the color of the color box (an Image component)
        Image colorBox = legendItem.transform.Find("ColorBox").GetComponent<Image>();
        colorBox.color = color;

        // Set the category name in the label (a TMP_Text component)
        TMP_Text label = legendItem.transform.Find("Label").GetComponent<TMP_Text>();
        label.text = categoryName;

        // Round the percentage to the nearest whole number and set it in the LabelPercentage TMP_Text component
        TMP_Text percentageLabel = legendItem.transform.Find("LabelPercentage").GetComponent<TMP_Text>();
        percentageLabel.text = $"{Mathf.Round(percentage)}%";
    }

    private void CalculateTotalGanancias(string[] preciosPlatos, string[] n)
    {
        float totalGanancias = 0;
        for (int i = 0; i < preciosPlatos.Length; i++)
        {
            totalGanancias += float.Parse(preciosPlatos[i]) * int.Parse(n[i]);
        }
        totalGananciasText.text = $"{totalGanancias:F2} €";
    }

    private void CalculateTotalPedidos(string[] n)
    {
        int totalPedidos = 0;
        for (int i = 0; i < n.Length; i++)
        {
            totalPedidos += int.Parse(n[i]);
        }
        totalPedidosText.text = $"{totalPedidos}";
    }

    private void DisplayTopPlatos(string[] platos, string[] n, string[] preciosPlatos, int sortIndex = 0)
    {
        if (platos == null || platos.Length == 0)
        {
            Debug.LogWarning("No data available for platos.");
            return;
        }

        Dictionary<string, (int orders, float earnings)> platosWithOrders = new Dictionary<string, (int, float)>();

        for (int i = 0; i < platos.Length; i++)
        {
            if (platosWithOrders.ContainsKey(platos[i]))
            {
                platosWithOrders[platos[i]] = (
                    platosWithOrders[platos[i]].orders + int.Parse(n[i]),
                    platosWithOrders[platos[i]].earnings + (float.Parse(preciosPlatos[i]) * int.Parse(n[i]))
                );
            }
            else
            {
                platosWithOrders[platos[i]] = (int.Parse(n[i]), float.Parse(preciosPlatos[i]) * int.Parse(n[i]));
            }
        }

        List<(string plato, int orders, float earnings)> sortedPlatos = new List<(string, int, float)>();
        foreach (var entry in platosWithOrders)
        {
            sortedPlatos.Add((entry.Key, entry.Value.orders, entry.Value.earnings));
        }

        // Sort based on selected criteria
        switch (sortIndex)
        {
            case 0:
                sortedPlatos.Sort((a, b) => b.orders.CompareTo(a.orders));
                break;
            case 1:
                sortedPlatos.Sort((a, b) => a.orders.CompareTo(b.orders));
                break;
            case 2:
                sortedPlatos.Sort((a, b) => b.earnings.CompareTo(a.earnings));
                break;
            case 3:
                sortedPlatos.Sort((a, b) => a.earnings.CompareTo(b.earnings));
                break;
            default:
                sortedPlatos.Sort((a, b) => b.orders.CompareTo(a.orders));
                break;
        }

        // Clear previous items
        foreach (Transform child in topPlatosPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Display the sorted platos
        foreach (var platoInfo in sortedPlatos)
        {
            GameObject topPlatoItem = Instantiate(topPlatoItemPrefab, topPlatosPanel.transform);
            TMP_Text dishNameText = topPlatoItem.transform.Find("DishName").GetComponent<TMP_Text>();
            TMP_Text orderCountText = topPlatoItem.transform.Find("Orders").GetComponent<TMP_Text>();
            TMP_Text earningsText = topPlatoItem.transform.Find("Earnings").GetComponent<TMP_Text>();

            dishNameText.text = platoInfo.plato;
            orderCountText.text = platoInfo.orders.ToString();
            earningsText.text = $"{platoInfo.earnings:F2} €";
        }
    }

    private void DisplayTopMesas(int[] mesas, string[] n, string[] preciosPlatos, int sortIndex = 0)
    {
        if (mesas == null || mesas.Length == 0)
        {
            Debug.LogWarning("No data available for mesas.");
            return;
        }

        Dictionary<int, (int orders, float earnings)> mesasWithOrders = new Dictionary<int, (int, float)>();

        for (int i = 0; i < mesas.Length; i++)
        {
            if (mesasWithOrders.ContainsKey(mesas[i]))
            {
                mesasWithOrders[mesas[i]] = (
                    mesasWithOrders[mesas[i]].orders + int.Parse(n[i]),
                    mesasWithOrders[mesas[i]].earnings + (float.Parse(preciosPlatos[i]) * int.Parse(n[i]))
                );
            }
            else
            {
                mesasWithOrders[mesas[i]] = (int.Parse(n[i]), float.Parse(preciosPlatos[i]) * int.Parse(n[i]));
            }
        }

        List<(int mesa, int orders, float earnings)> sortedMesas = new List<(int, int, float)>();
        foreach (var entry in mesasWithOrders)
        {
            sortedMesas.Add((entry.Key, entry.Value.orders, entry.Value.earnings));
        }

        switch (sortIndex)
        {
            case 0:
                sortedMesas.Sort((a, b) => b.orders.CompareTo(a.orders));
                break;
            case 1:
                sortedMesas.Sort((a, b) => a.orders.CompareTo(b.orders));
                break;
            case 2:
                sortedMesas.Sort((a, b) => b.earnings.CompareTo(a.earnings));
                break;
            case 3:
                sortedMesas.Sort((a, b) => a.earnings.CompareTo(b.earnings));
                break;
            default:
                sortedMesas.Sort((a, b) => b.orders.CompareTo(a.orders));
                break;
        }

        foreach (Transform child in topMesasPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var mesaInfo in sortedMesas)
        {
            GameObject topMesaItem = Instantiate(topMesaItemPrefab, topMesasPanel.transform);
            TMP_Text tableNameText = topMesaItem.transform.Find("TableName").GetComponent<TMP_Text>();
            TMP_Text ordersText = topMesaItem.transform.Find("Orders").GetComponent<TMP_Text>();
            TMP_Text earningsText = topMesaItem.transform.Find("Earnings").GetComponent<TMP_Text>();

            tableNameText.text = mesaInfo.mesa.ToString();
            ordersText.text = mesaInfo.orders.ToString();
            earningsText.text = $"{mesaInfo.earnings:F2} €";
        }
    }

    private void GenerateBarChart(string[] fechas, string[] n)
    {
        if (fechas == null || fechas.Length == 0)
        {
            Debug.LogWarning("No fecha data available.");
            return;
        }

        int[] dayOfWeekCounts = new int[7];

        for (int i = 0; i < fechas.Length; i++)
        {
            DayOfWeek dayOfWeek = GetDayOfWeekFromFecha(fechas[i]);
            int dayIndex = ((int)dayOfWeek + 6) % 7; // Adjust so Monday = 0, Sunday = 6
            dayOfWeekCounts[dayIndex] += int.Parse(n[i]);
        }

        string[] daysOfWeek = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
        List<float> barHeights = new List<float>();

        for (int i = 0; i < dayOfWeekCounts.Length; i++)
        {
            barHeights.Add(dayOfWeekCounts[i]);
        }

        barChartTest.UpdateBarChart(barHeights.ToArray(), new Color[dayOfWeekCounts.Length]);
    }

    private void GeneratePlot(string[] fechas, string[] n, string[] preciosPlatos, DataFilterMode mode)
    {
        List<string> labels = new List<string>();

        if (fechas == null || fechas.Length == 0)
        {
            Debug.LogWarning("No fecha data available.");
            return;
        }
        
        // CASE 1: Current Month or Last Month
        if (mode == DataFilterMode.CurrentMonth || mode == DataFilterMode.LastMonth)
        {
            Dictionary<int, (int totalPedidos, float totalEarnings)> dailyData = new Dictionary<int, (int, float)>();
            int refMonth = 0, refYear = 0;
            bool first = true;

            // Aggregate data per day.
            for (int i = 0; i < fechas.Length; i++)
            {
                DateTime dt;
                if (DateTime.TryParseExact(fechas[i], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                {
                    if (first)
                    {
                        refMonth = dt.Month;
                        refYear = dt.Year;
                        first = false;
                    }
                    int day = dt.Day;
                    if (!dailyData.ContainsKey(day))
                        dailyData[day] = (0, 0f);
                    dailyData[day] = (dailyData[day].totalPedidos + int.Parse(n[i]),
                                    dailyData[day].totalEarnings + float.Parse(preciosPlatos[i]) * int.Parse(n[i]));
                }
                else
                {
                    Debug.LogWarning($"Invalid date format: {fechas[i]}");
                }
            }

            // Ensure a valid reference month/year.
            if (refMonth == 0 || refYear == 0)
            {
                DateTime now = DateTime.Now;
                refMonth = now.Month;
                refYear = now.Year;
            }

            int totalDays = DateTime.DaysInMonth(refYear, refMonth);

            // Clear the labels list if needed.
            labels.Clear();

            // Create lists for xData and yData, covering every day.
            List<float> xDataList = new List<float>();
            List<float> yDataList = new List<float>();

            for (int day = 1; day <= totalDays; day++)
            {
                xDataList.Add(day); // day-of-month (e.g., 1, 2, 3, ...)
                if (dailyData.ContainsKey(day))
                {
                    yDataList.Add(dailyData[day].totalEarnings);
                }
                else
                {
                    yDataList.Add(0f);
                }
                labels.Add(day.ToString());
            }

            simplePlot.xData = xDataList.ToArray();
            simplePlot.yData = yDataList.ToArray();
            simplePlot.xMin = 1;
            simplePlot.xMax = totalDays;

            float maxEarnings = yDataList.Count > 0 ? yDataList.Max() : 0f;
            simplePlot.yMin = 0f;
            simplePlot.yMax = maxEarnings * 1.1f;
        }

        // CASE 2: Last 6 Months, Last 12 Months, or Lifetime
        else if (mode == DataFilterMode.Last6Months || mode == DataFilterMode.Last12Months || mode == DataFilterMode.Lifetime)
        {
            Dictionary<int, (int totalPedidos, float totalEarnings)> monthlyData = new Dictionary<int, (int, float)>();
            for (int i = 0; i < fechas.Length; i++)
            {
                DateTime dt;
                if (DateTime.TryParseExact(fechas[i], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                {
                    int key = dt.Year * 100 + dt.Month; // e.g., 202303 for March 2023
                    if (!monthlyData.ContainsKey(key))
                        monthlyData[key] = (0, 0f);
                    monthlyData[key] = (monthlyData[key].totalPedidos + int.Parse(n[i]),
                                        monthlyData[key].totalEarnings + float.Parse(preciosPlatos[i]) * int.Parse(n[i]));
                }
                else
                {
                    Debug.LogWarning($"Invalid date format: {fechas[i]}");
                }
            }

            // Sort the keys chronologically
            var sortedKeys = monthlyData.Keys.ToList();
            sortedKeys.Sort();

            List<float> xDataList = new List<float>();
            List<float> yDataList = new List<float>();
            for (int i = 0; i < sortedKeys.Count; i++)
            {
                xDataList.Add(i + 1); // 1, 2, 3, ...
                yDataList.Add(monthlyData[sortedKeys[i]].totalEarnings);
            }

            simplePlot.xData = xDataList.ToArray();
            simplePlot.yData = yDataList.ToArray();
            simplePlot.xMin = 1;
            simplePlot.xMax = sortedKeys.Count;

            float maxEarnings = yDataList.Count > 0 ? yDataList.Max() : 0f;
            simplePlot.yMin = 0f;
            simplePlot.yMax = maxEarnings * 1.1f;

            foreach (int key in sortedKeys)
            {
                int year = key / 100;
                int month = key % 100;
                labels.Add(new DateTime(year, month, 1).ToString("MMM yyyy")); // "Jan 2023", "Feb 2023"
            }
        }

        simplePlot.xLabels = labels; // Assign labels before drawing the plot
        simplePlot.DrawPlot();
    }

    private DayOfWeek GetDayOfWeekFromFecha(string fecha)
    {
        DateTime date;
        // Assuming the date format is "dd/MM/yyyy"
        if (DateTime.TryParseExact(fecha, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
        {
            return date.DayOfWeek;
        }
        else
        {
            Debug.LogWarning($"Invalid date format: {fecha}");
            return DayOfWeek.Monday; // Default to Monday or handle as needed
        }
    }

    public void SetFilter(int filterIndex)
    {
        // Map the index to the DataFilterMode enum
        DataFilterMode mode;
        switch (filterIndex)
        {
            case 0:
                mode = DataFilterMode.Lifetime;
                break;
            case 1:
                mode = DataFilterMode.CurrentMonth;
                break;
            case 2:
                mode = DataFilterMode.LastMonth;
                break;
            case 3:
                mode = DataFilterMode.Last6Months;
                break;
            case 4:
                mode = DataFilterMode.Last12Months;
                break;
            default:
                mode = DataFilterMode.Lifetime;
                break;
        }

        RefreshData(mode);
    }

    /// <summary>
    /// Refresh the filtered arrays and update charts, totals, and top lists.
    /// </summary>
    public void RefreshData(DataFilterMode mode)
    {
        if (!DataBaseRegistros.isDataLoaded)
        {
            Debug.LogWarning("Data is not loaded yet.");
            return;
        }

        // Get indices of records that pass the filter
        List<int> indices = GetFilteredIndices(mode);

        if (indices.Count == 0)
        {
            noHayDatosParaEsteFiltro.SetActive(true);

            // Clear PieChart
            piechartTest.UpdatePieChart(new float[] { 1f }, new Color[] { Color.clear }); // dummy slice

            // Clear PieChart legend
            foreach (Transform child in legendPanel.transform)
                Destroy(child.gameObject);

            // Clear BarChart with 7 zeros (Monday-Sunday)
            barChartTest.UpdateBarChart(new float[] { 0.01f, 0.01f, 0.01f, 0.01f, 0.01f, 0.01f, 0.01f },
                            new Color[] { Color.clear, Color.clear, Color.clear, Color.clear, Color.clear, Color.clear, Color.clear });

            // Clear SimplePlot
            simplePlot.xData = new float[] { 1f };
            simplePlot.yData = new float[] { 0f };
            simplePlot.xLabels = new List<string> { "No Data" };
            simplePlot.DrawPlot();

            // Clear totals
            totalGananciasText.text = "0 €";
            totalPedidosText.text = "0";

            // Clear top lists
            foreach (Transform child in topPlatosPanel.transform) Destroy(child.gameObject);
            foreach (Transform child in topMesasPanel.transform) Destroy(child.gameObject);

            // Clear class-level arrays
            filteredN = new string[0];
            filteredPreciosPlatos = new string[0];
            filteredPlato = new string[0];
            filteredMesa = new int[0];

            return; // stop further chart updates
        }
        else
        {
            noHayDatosParaEsteFiltro.SetActive(false);
        }


        // Filtered arrays
        string[] filteredFechas = indices.Select(i => DataBaseRegistros.fecha[i]).ToArray();
        string[] filteredCategorias = indices
            .SelectMany(i => DataBaseRegistros.categoria[i].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        string[] filteredNLocal = indices
            .SelectMany(i => DataBaseRegistros.n[i].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        string[] filteredPreciosPlatosLocal = indices
            .SelectMany(i => DataBaseRegistros.precioPlato[i].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        // Assign directly to the class-level field (no 'string[]' here)
        filteredPlato = indices
            .SelectMany(i => DataBaseRegistros.plato[i].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        filteredMesa = indices.Select(i => DataBaseRegistros.mesa[i]).ToArray();

        // Also assign to class-level arrays for dropdown callbacks
        filteredN = filteredNLocal;
        filteredPreciosPlatos = filteredPreciosPlatosLocal;

        // Update all charts and totals
        GenerateChart(filteredCategorias, filteredNLocal);
        GenerateBarChart(filteredFechas, filteredNLocal);
        GeneratePlot(filteredFechas, filteredNLocal, filteredPreciosPlatosLocal, currentFilter);
        CalculateTotalGanancias(filteredPreciosPlatosLocal, filteredNLocal);
        CalculateTotalPedidos(filteredNLocal);

        // Update top lists, using current dropdown values (or default sort index 0)
        DisplayTopPlatos(filteredPlato, filteredNLocal, filteredPreciosPlatosLocal, sortingPlatosDropdown.value);
        DisplayTopMesas(filteredMesa, filteredNLocal, filteredPreciosPlatosLocal, sortingMesasDropdown.value);
    }
}
