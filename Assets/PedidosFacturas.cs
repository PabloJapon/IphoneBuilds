using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;
using Font = iTextSharp.text.Font;
using System.Linq;

public class PedidosFacturas : MonoBehaviour
{
    public string url;

    public TPV_DataManager tpvDataManager;

    public TMP_InputField buscarPedido;
    public static string[] fecha;
    public static string[] nPedido;
    public static float[] precio;
    public static int[] mesa;
    public static string[] plato;
    public static string[] precioPlato;
    public static string[] n;

    public Transform contenedorPrefabs;
    public GameObject prefabProducto;

    public TMP_Text id;

    public GameObject prefabRegistrosFacturas;
    public GameObject contentRegistrosFacturas;

    private string lastJson = "";

    // Crear factura
    public GameObject canvasCrearFactura;
    public TMP_InputField inputFieldFechaExp;
    public TMP_InputField inputFieldFechaOp;
    public TMP_Text textSubtotal;
    public TMP_Text textIVA;
    public TMP_Text textCantidadTotal;
    public TMP_InputField inputFieldNFactura;
    public TMP_InputField inputFieldNPedido;
    private string nFactura = "";

    // Datos emisor PDF
    public string emisorNombre = "Empresa S.L.";
    public string emisorNIF = "B12345678";
    public string emisorDireccion = "Calle Falsa 123, Ciudad";

    // Datos cliente factura
    public TMP_InputField inputFieldNombreCliente;
    public TMP_InputField inputFieldNifCliente;
    public TMP_InputField inputFieldDireccionCliente;

    // Suggestion panel for cliente search inside canvasCrearFactura
    public GameObject suggestionPanelCliente;
    public GameObject suggestionPrefabCliente;

    public TMP_Text textFolder;
    public GameObject canvasAbrirCarpeta;

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    void Start()
    {
        buscarPedido.onValueChanged.AddListener(delegate { FiltrarRegistros(); });
        inputFieldNombreCliente.onValueChanged.AddListener(delegate { OnClienteFacturaInputChanged(); });
        StartCoroutine(CheckForUpdatesRoutine());
    }

    // =========================================================
    // DATA FETCHING
    // =========================================================

    IEnumerator CheckForUpdatesRoutine()
    {
        while (true)
        {
            if (id != null && !string.IsNullOrEmpty(id.text))
                yield return StartCoroutine(LoadPersonalizacionDataFacturas());

            yield return new WaitForSeconds(10f);
        }
    }

    public IEnumerator LoadPersonalizacionDataFacturas()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/registros_pedidos/restaurant/" + id.text);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to fetch Registros Pedidos: " + request.error);
            yield break;
        }

        string newJson = request.downloadHandler.text;

        if (newJson == lastJson)
            yield break;

        lastJson = newJson;

        List<PersonalizacionEntryFacturas> entries = ParsePersonalizacion(newJson);

        entries = entries
            .OrderByDescending(e => DateTime.ParseExact(
                e.fecha + " " + e.hora,
                "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture))
            .ToList();

        int count = entries.Count;
        fecha = new string[count];
        precio = new float[count];
        mesa = new int[count];
        nPedido = new string[count];
        n = new string[count];
        plato = new string[count];
        precioPlato = new string[count];

        foreach (Transform child in contentRegistrosFacturas.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < count; i++)
        {
            PersonalizacionEntryFacturas entry = entries[i];
            fecha[i] = entry.fecha;
            precio[i] = entry.precio;
            mesa[i] = entry.mesa;
            nPedido[i] = entry.nPedido;
            n[i] = entry.n;
            plato[i] = entry.plato;
            precioPlato[i] = entry.precioPlato;

            GameObject instance = Instantiate(prefabRegistrosFacturas, contentRegistrosFacturas.transform, false);

            TMP_Text[] texts = instance.GetComponentsInChildren<TMP_Text>();
            texts[0].text = entry.fecha;
            texts[1].text = entry.nPedido;
            texts[2].text = entry.precio.ToString("F2").Replace('.', ',') + " €";

            if (entry.mesa > 1999)
                texts[3].text = "Delivery";
            else if (entry.mesa > 999)
                texts[3].text = "Recoger";
            else
                texts[3].text = entry.mesa.ToString();

            int capturedIndex = i;
            instance.GetComponentInChildren<Button>().onClick.AddListener(() =>
                CrearFactura(fecha[capturedIndex], precio[capturedIndex], nPedido[capturedIndex],
                             plato[capturedIndex], precioPlato[capturedIndex], n[capturedIndex]));
        }
    }

    // =========================================================
    // CREAR FACTURA
    // =========================================================

    public void CrearFactura(string fecha, float precio, string nPedido, string plato, string precioPlato, string n)
    {
        canvasCrearFactura.SetActive(true);

        inputFieldFechaExp.text = DateTime.Now.ToString("dd/MM/yyyy");
        inputFieldFechaOp.text = fecha;
        inputFieldNPedido.text = nPedido;

        // Fetch number from server
        StartCoroutine(FetchNextFacturaNumber(nFacturaResult =>
        {
            inputFieldNFactura.text = nFacturaResult;
            nFactura = nFacturaResult; // keep the class variable in sync for PDF
        }));

        CrearListaProductos(plato, precioPlato, n);

        float subtotal = precio / 1.1f;
        float iva = subtotal * 0.10f;
        float totalFinal = subtotal + iva;

        textSubtotal.text = subtotal.ToString("F2").Replace('.', ',') + " €";
        textIVA.text = iva.ToString("F2").Replace('.', ',') + " €";
        textCantidadTotal.text = totalFinal.ToString("F2").Replace('.', ',') + " €";
    }

    public void CrearListaProductos(string platosStr, string preciosStr, string cantStr)
    {
        string[] platos = platosStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        string[] precios = preciosStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        string[] cantidades = cantStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        int itemCount = Mathf.Min(platos.Length, precios.Length, cantidades.Length);

        foreach (Transform child in contenedorPrefabs)
            Destroy(child.gameObject);

        for (int i = 0; i < itemCount; i++)
        {
            GameObject nuevoItem = Instantiate(prefabProducto, contenedorPrefabs);

            TMP_Text[] texts = nuevoItem.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 4)
            {
                float precio = float.Parse(precios[i], CultureInfo.InvariantCulture);
                int cantidad = int.Parse(cantidades[i]);
                float total = precio * cantidad;

                texts[0].text = platos[i];
                texts[1].text = precio.ToString("0.00").Replace('.', ',') + " €";
                texts[2].text = cantidad.ToString();
                texts[3].text = total.ToString("0.00").Replace('.', ',') + " €";
            }
            else
            {
                Debug.LogError("El prefab no contiene 4 componentes TMP_Text como se esperaba.");
            }
        }
    }

    // =========================================================
    // CLIENTE SUGGESTION (inside canvasCrearFactura)
    // =========================================================

    public void OnClienteFacturaInputChanged()
    {
        string input = inputFieldNombreCliente.text.ToLower().Trim();

        foreach (Transform child in suggestionPanelCliente.transform)
            Destroy(child.gameObject);

        if (input.Length == 0)
        {
            suggestionPanelCliente.SetActive(false);
            return;
        }

        suggestionPanelCliente.SetActive(true);

        List<TPV_DataManager.Customer> filtered =
            tpvDataManager.tpvData.customers.FindAll(c =>
                !string.IsNullOrEmpty(c.name) &&
                c.name.ToLower().Contains(input));

        foreach (var customer in filtered)
        {
            GameObject go = Instantiate(suggestionPrefabCliente, suggestionPanelCliente.transform);

            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{customer.name} - {customer.nif} - {customer.address}";

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                inputFieldNombreCliente.text = customer.name;
                inputFieldNifCliente.text = customer.nif;
                inputFieldDireccionCliente.text = customer.address;
                suggestionPanelCliente.SetActive(false);
            });
        }
    }

    // =========================================================
    // CREAR PDF
    // =========================================================

    public void ConfirmFactura()
    {
        StartCoroutine(ConfirmFacturaSendNumberFacturaToDatabase());
    }

    IEnumerator ConfirmFacturaSendNumberFacturaToDatabase()
    {
        UnityWebRequest request = new UnityWebRequest(url + "/facturas/confirm_number/" + id.text, "POST");
        request.uploadHandler = new UploadHandlerRaw(new byte[1] { 0 });
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Error confirming factura number: " + request.error);
            yield break;
        }

        FacturaNumberResponse response = JsonUtility.FromJson<FacturaNumberResponse>(
            request.downloadHandler.text);

        nFactura = response.nFactura;
        inputFieldNFactura.text = nFactura;
    }

    public void CrearFacturaPDF()
    {
        string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Facturas");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string pdfPath = Path.Combine(folderPath, $"Factura_{nFactura}.pdf");

        Document document = new Document(PageSize.A4, 36, 36, 36, 36);
        PdfWriter.GetInstance(document, new FileStream(pdfPath, FileMode.Create));
        document.Open();

        Font fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
        Font fontSub = FontFactory.GetFont(FontFactory.HELVETICA, 12);
        Font fontLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        Font fontText = FontFactory.GetFont(FontFactory.HELVETICA, 12);

        Paragraph titulo = new Paragraph("Factura", fontTitle);
        titulo.Alignment = Element.ALIGN_LEFT;
        titulo.SpacingAfter = 10f;
        document.Add(titulo);

        document.Add(new Paragraph("Nº factura: " + nFactura, fontSub));
        document.Add(new Paragraph("Nº pedido: " + inputFieldNPedido.text, fontSub));
        document.Add(new Paragraph("Fecha de expedición: " + inputFieldFechaExp.text, fontSub));
        document.Add(new Paragraph("Fecha de la operación: " + inputFieldFechaOp.text, fontSub));
        document.Add(new Paragraph("\n"));

        PdfPTable datosTable = new PdfPTable(2);
        datosTable.WidthPercentage = 100;
        datosTable.SetWidths(new float[] { 1f, 1f });

        PdfPCell emisorCell = new PdfPCell();
        emisorCell.Border = Rectangle.NO_BORDER;
        emisorCell.AddElement(new Paragraph("Datos del emisor", fontLabel));
        emisorCell.AddElement(new Paragraph(emisorNombre, fontText));
        emisorCell.AddElement(new Paragraph(emisorNIF, fontText));
        emisorCell.AddElement(new Paragraph(emisorDireccion, fontText));

        PdfPCell clienteCell = new PdfPCell();
        clienteCell.Border = Rectangle.NO_BORDER;
        clienteCell.AddElement(new Paragraph("Datos del cliente", fontLabel));
        clienteCell.AddElement(new Paragraph(inputFieldNombreCliente.text, fontText));
        clienteCell.AddElement(new Paragraph(inputFieldNifCliente.text, fontText));
        clienteCell.AddElement(new Paragraph(inputFieldDireccionCliente.text, fontText));

        datosTable.AddCell(emisorCell);
        datosTable.AddCell(clienteCell);
        datosTable.SpacingAfter = 20f;
        document.Add(datosTable);

        PdfPTable tablaServicios = new PdfPTable(4);
        tablaServicios.WidthPercentage = 100;
        tablaServicios.SetWidths(new float[] { 3f, 1f, 1f, 1f });

        AddCell(tablaServicios, "Descripción", fontLabel, bottomBorder: true);
        AddCell(tablaServicios, "Precio", fontLabel, bottomBorder: true);
        AddCell(tablaServicios, "Cantidad", fontLabel, bottomBorder: true);
        AddCell(tablaServicios, "Total", fontLabel, bottomBorder: true);

        float sumaTotal = 0f;

        foreach (Transform item in contenedorPrefabs)
        {
            TMP_Text descripcion = item.Find("Descripcion").GetComponent<TMP_Text>();
            TMP_Text precioPDF = item.Find("Precio").GetComponent<TMP_Text>();
            TMP_Text cantidad = item.Find("Cantidad").GetComponent<TMP_Text>();
            TMP_Text total = item.Find("Total").GetComponent<TMP_Text>();

            AddCell(tablaServicios, descripcion.text, fontText);
            AddCell(tablaServicios, precioPDF.text, fontText);
            AddCell(tablaServicios, cantidad.text, fontText);
            AddCell(tablaServicios, total.text, fontText);

            string totalStr = total.text.Replace("€", "").Trim().Replace(",", ".");
            if (float.TryParse(totalStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float totalValue))
                sumaTotal += totalValue;
        }

        PdfPCell separator = new PdfPCell(new Phrase(""))
        {
            Border = Rectangle.BOTTOM_BORDER,
            Colspan = 4,
            PaddingTop = 5f,
            PaddingBottom = 5f
        };
        tablaServicios.AddCell(separator);
        tablaServicios.SpacingAfter = 10f;
        document.Add(tablaServicios);

        float subtotal = sumaTotal / 1.1f;
        float iva = subtotal * 0.10f;
        float totalFinal = subtotal + iva;

        document.Add(new Paragraph("Subtotal: " + subtotal.ToString("0.00") + " €", fontText));
        document.Add(new Paragraph("IVA (10%): " + iva.ToString("0.00") + " €", fontText));
        document.Add(new Paragraph("Total: " + totalFinal.ToString("0.00") + " €", fontLabel));

        // Guardar factura en JSON local
        TPV_DataManager.Invoice invoice = new TPV_DataManager.Invoice();
        invoice.id = nFactura;
        invoice.date = inputFieldFechaExp.text;

        // Buscar cliente por nombre
        TPV_DataManager.Customer cliente = tpvDataManager.tpvData.customers.Find(c =>
            c.name == inputFieldNombreCliente.text);
        invoice.customerId = cliente != null ? cliente.id : -1;

        // Añadir items
        foreach (Transform item in contenedorPrefabs)
        {
            TMP_Text descripcion = item.Find("Descripcion").GetComponent<TMP_Text>();
            TMP_Text precioItem = item.Find("Precio").GetComponent<TMP_Text>();
            TMP_Text cantidadItem = item.Find("Cantidad").GetComponent<TMP_Text>();

            float unitPrice = float.Parse(
                precioItem.text.Replace("€", "").Trim().Replace(",", "."),
                CultureInfo.InvariantCulture);
            int qty = int.Parse(cantidadItem.text);

            invoice.items.Add(new TPV_DataManager.InvoiceItem
            {
                description = descripcion.text,
                quantity = qty,
                unitPrice = unitPrice,
                taxRate = 0.10f
            });
        }

        // Totales
        float sub = sumaTotal / 1.1f;
        invoice.subtotal = sub;
        invoice.taxAmount = sub * 0.10f;
        invoice.total = sumaTotal;

        // Finalizar con hash y audit log
        tpvDataManager.FinalizeInvoice(invoice, emisorNombre);
        tpvDataManager.tpvData.invoices.Add(invoice);
        tpvDataManager.SaveData();

        document.Close();

        Debug.Log("Factura PDF creada correctamente en: " + pdfPath);
        canvasAbrirCarpeta.SetActive(true);
        textFolder.text = "Factura guardada en: " + pdfPath;
    }

    void AddCell(PdfPTable table, string text, Font font, bool bottomBorder = false)
    {
        PdfPCell cell = new PdfPCell(new Phrase(text, font));
        cell.Border = bottomBorder ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;
        cell.Padding = 5f;
        table.AddCell(cell);
    }

    // =========================================================
    // PARSING
    // =========================================================

    public List<PersonalizacionEntryFacturas> ParsePersonalizacion(string json)
    {
        List<PersonalizacionEntryFacturas> entries = new List<PersonalizacionEntryFacturas>();

        string wrappedJson = "{ \"items\": " + json + " }";
        PersonalizacionDataList5 data = JsonUtility.FromJson<PersonalizacionDataList5>(wrappedJson);

        foreach (var item in data.items)
        {
            entries.Add(new PersonalizacionEntryFacturas(
                item.fecha, item.hora, item.precio, item.mesa,
                item.nPedido, item.n, item.plato, item.precioPlato));
        }

        return entries;
    }

    [Serializable]
    public class PersonalizacionFacturas
    {
        public string fecha;
        public string hora;
        public float precio;
        public int mesa;
        public string nPedido;
        public string n;
        public string plato;
        public string precioPlato;
    }

    [Serializable]
    public class PersonalizacionDataList5
    {
        public PersonalizacionFacturas[] items;
    }

    public class PersonalizacionEntryFacturas
    {
        public string fecha { get; private set; }
        public string hora { get; private set; }
        public float precio { get; private set; }
        public int mesa { get; private set; }
        public string nPedido { get; private set; }
        public string n { get; private set; }
        public string plato { get; private set; }
        public string precioPlato { get; private set; }

        public PersonalizacionEntryFacturas(string fecha, string hora, float precio, int mesa,
            string nPedido, string n, string plato, string precioPlato)
        {
            this.fecha = fecha;
            this.hora = hora;
            this.precio = precio;
            this.mesa = mesa;
            this.nPedido = nPedido;
            this.n = n;
            this.plato = plato;
            this.precioPlato = precioPlato;
        }
    }

    // =========================================================
    // FILTRAR REGISTROS
    // =========================================================

    public void FiltrarRegistros()
    {
        string filtro = buscarPedido.text.ToLower().Trim();

        foreach (Transform hijo in contentRegistrosFacturas.transform)
        {
            TMP_Text[] texts = hijo.GetComponentsInChildren<TMP_Text>();
            bool coincideFecha = texts[0].text.ToLower().Contains(filtro);
            bool coincidePedido = texts[1].text.ToLower().Contains(filtro);
            hijo.gameObject.SetActive(coincideFecha || coincidePedido);
        }
    }

    // Facturas numbering
    IEnumerator FetchNextFacturaNumber(System.Action<string> onResult)
    {
        Debug.Log("Fetching: " + url + "/facturas/next_number/" + id.text);
        UnityWebRequest request = UnityWebRequest.Get(url + "/facturas/next_number/" + id.text);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Error fetching factura number: " + request.error);
            onResult("ERROR");
            yield break;
        }

        // Parse {"nFactura": "2025-0001"}
        string json = request.downloadHandler.text;
        FacturaNumberResponse response = JsonUtility.FromJson<FacturaNumberResponse>(json);
        onResult(response.nFactura);
    }

    [System.Serializable]
    private class FacturaNumberResponse
    {
        public string nFactura;
    }
}