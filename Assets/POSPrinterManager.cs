using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.IO.Ports;
using System.Text.RegularExpressions;

public class POSPrinterManager : MonoBehaviour
{
    private string defaultPrinter;

    public bool isTPV
    {
        get => PlayerPrefs.GetInt("Gastrali_IsTPV", 0) == 1;
        set { PlayerPrefs.SetInt("Gastrali_IsTPV", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public string serialPortName
    {
        get => PlayerPrefs.GetString("Gastrali_PrinterPort", "");
        set { PlayerPrefs.SetString("Gastrali_PrinterPort", value); PlayerPrefs.Save(); }
    }

    public List<string> GetAvailableSerialPorts()
    {
        return new List<string>(SerialPort.GetPortNames());
    }

    private void SendBytesToPrinterSerial(byte[] data)
{
    if (string.IsNullOrEmpty(serialPortName))
    {
        Debug.LogError("No hay puerto de impresora configurado. Usa Ajustes > Impresora para seleccionarlo.");
        return;
    }

    try
    {
        using (var sp = new SerialPort(serialPortName, 9600, Parity.None, 8, StopBits.One))
        {
            sp.Open();
            sp.Write(data, 0, data.Length);
        }
        Debug.Log("Serial print OK, bytes=" + data.Length);
    }
    catch (Exception e)
    {
        Debug.LogError("Serial print FAILED: " + e.Message);
    }
}

    private static class RawSerial
    {
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;

        [StructLayout(LayoutKind.Sequential)]
        private struct DCB
        {
            public int DCBlength;
            public int BaudRate;
            public int fFlags;
            public short wReserved;
            public short XonLim;
            public short XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public byte XonChar;
            public byte XoffChar;
            public byte ErrorChar;
            public byte EofChar;
            public byte EvtChar;
            public short wReserved1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct COMMTIMEOUTS
        {
            public uint ReadIntervalTimeout;
            public uint ReadTotalTimeoutMultiplier;
            public uint ReadTotalTimeoutConstant;
            public uint WriteTotalTimeoutMultiplier;
            public uint WriteTotalTimeoutConstant;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(
            IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCommState(IntPtr hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommState(IntPtr hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool BuildCommDCB(string lpDef, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommTimeouts(IntPtr hFile, ref COMMTIMEOUTS lpCommTimeouts);

        public static bool Write(string portName, byte[] data, out string error)
        {
            error = null;
            IntPtr handle = CreateFile(@"\\.\" + portName, GENERIC_READ | GENERIC_WRITE, 0,
                IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (handle == new IntPtr(-1))
            {
                error = "CreateFile failed. Win32Error=" + Marshal.GetLastWin32Error();
                return false;
            }

            DCB dcb = new DCB();
            dcb.DCBlength = Marshal.SizeOf(dcb);
            if (!GetCommState(handle, ref dcb) ||
                !BuildCommDCB("baud=9600 parity=N data=8 stop=1", ref dcb) ||
                !SetCommState(handle, ref dcb))
            {
                error = "SetCommState failed. Win32Error=" + Marshal.GetLastWin32Error();
                CloseHandle(handle);
                return false;
            }

            COMMTIMEOUTS timeouts = new COMMTIMEOUTS
            {
                ReadIntervalTimeout = 50,
                ReadTotalTimeoutMultiplier = 10,
                ReadTotalTimeoutConstant = 100,
                WriteTotalTimeoutMultiplier = 10,
                WriteTotalTimeoutConstant = 3000
            };
            SetCommTimeouts(handle, ref timeouts);

            bool ok = WriteFile(handle, data, (uint)data.Length, out uint written, IntPtr.Zero);
            if (!ok)
                error = "WriteFile failed. Win32Error=" + Marshal.GetLastWin32Error();
            else if (written != data.Length)
                error = "WriteFile incomplete: " + written + "/" + data.Length;

            CloseHandle(handle);
            return ok && written == data.Length;
        }
    }

    public TMP_Text restId;
    public TMP_Text numeroMesa;
    public TMP_Text nombreCamarero;
    public TMP_Text nombreRestaurante;

    public CrearCamarero CC;
    private MesaData savedData;

    [Header("Facturas")]
    public TMP_InputField nombreCliente;
    public TMP_InputField direccionCliente;
    public TMP_InputField NIFCliente;

    public TMP_InputField fechaOperacion;
    public TMP_InputField fechaExpedicion;
    public TMP_InputField numeroFactura;
    public TMP_InputField numeroPedido;

    public GameObject contenedorProductos;

    [Header("Reportes")]
    public TMP_Text reporteXFechaApertura;
    public TMP_Text reporteXAbiertoPor;
    public TMP_Text reporteXFondoInicial;
    public TMP_Text reporteXIngresosEfectivo;
    public TMP_Text reporteXIngresosTarjeta;
    public TMP_Text reporteXDepositos;
    public TMP_Text reporteXRetiros;
    public TMP_Text reporteXEfectivoActual;

    public TMP_Text reporteZFechaApertura;
    public TMP_Text reporteZAbiertoPor;
    public TMP_Text reporteZFechaCierre;
    public TMP_Text reporteZCerradoPor;
    public TMP_Text reporteZFondoInicial;
    public TMP_Text reporteZIngresosEfectivo;
    public TMP_Text reporteZIngresosTarjeta;
    public TMP_Text reporteZDepositos;
    public TMP_Text reporteZRetiros;
    public TMP_Text reporteZEfectivoActual;
    public TMP_Text reporteZFinalTeorico;
    public TMP_Text reporteZFinalReal;
    public TMP_Text reporteZDescuadre;


    public static POSPrinterManager instance;

    void Awake()
    {
        instance = this;
        defaultPrinter = GetDefaultPrinterName();
        if (string.IsNullOrEmpty(serialPortName))
            Debug.LogWarning("No hay puerto de impresora configurado en este PC. Usa Ajustes > Impresora.");
        /* else
            Debug.Log("Puerto serie guardado en uso: " + serialPortName); */
    }

    public void PrintTestRemote(int mesaNumber)
    {
        if (SceneManager.GetActiveScene().name != "TPVScene")
            return; // not the TPV, nothing to do

        if (string.IsNullOrEmpty(defaultPrinter))
            return; // TPV scene but no printer configured

        // Change numeroMesa for the one waiter sent
        numeroMesa.text = mesaNumber.ToString();

        PrintTest();
    }

    public class Producto
    {
        public int Cantidad;
        public string Descripcion;
        public decimal PrecioUnitario;
        public string Opciones;
    }

    public void PrintTest()
    {
        // Retrieve saved data
        if (!CC.mesaContentSyncDictionary.TryGetValue(int.Parse(numeroMesa.text), out var contentSync))
        {
            Debug.LogError("FAILED: mesaContentSyncDictionary did not find mesa: " + numeroMesa.text);
            return;
        }
        if (!MesaStateManager.instance.TryGetContentState(restId.text, int.Parse(numeroMesa.text), out MesaData tmpData))
        {
            Debug.LogError("FAILED: TryGetContentState failed for restId: " + restId.text + " mesa: " + numeroMesa.text);
            return;
        }

        savedData = tmpData;

        // Determine count safely
        int itemCount = 0;

        // Check if nombrePlatoString is not null
        if (savedData.nombrePlatoString == null)
        {
            Debug.LogError("savedData.nombrePlatoString is null!");
        }
        else
        {
            // Handle List or array
            if (savedData.nombrePlatoString is System.Collections.IList list)
                itemCount = list.Count;
            else
                Debug.LogError("savedData.nombrePlatoString is neither List nor array!");
        }

        // Build productos list
        var productos = new List<Producto>();

        for (int i = 0; i < itemCount; i++)
        {
            int cantidad = int.Parse(savedData.cantidadPlatoString[i]);

            string priceRaw = Regex.Replace(savedData.precioPlatoString[i], @"[^\d,.\-]", "").Replace(",", ".").Trim();
            decimal precioTotal = decimal.TryParse(priceRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : 0;
            Debug.Log($"[PrintTest] item {i}: raw='{savedData.precioPlatoString[i]}' cleaned='{priceRaw}' parsed={p}");
            decimal precio = cantidad > 0 ? precioTotal / cantidad : precioTotal;

            string descripcion = savedData.nombrePlatoString[i];
            if (!string.IsNullOrWhiteSpace(savedData.opcionesPlato[i]))
                descripcion += " (" + savedData.opcionesPlato[i] + ")";

            string opciones ="";

            productos.Add(new Producto
            {
                Cantidad = cantidad,
                Descripcion = descripcion,
                PrecioUnitario = precio,
                Opciones = opciones,
            });
        }

        PrintTicket(int.Parse(numeroMesa.text), productos, nombreCamarero.text.Replace("Hola, ", ""), nombreRestaurante.text);
    }
    public void PrintTestFactura()
    {
        // Build productos list
        var productos = new List<Producto>();

        foreach (Transform producto in contenedorProductos.transform)
        {
            if (!producto.gameObject.activeInHierarchy)
            {
                Debug.Log($"[PrintTestFactura] Skipping inactive row: {producto.name}");
                continue;
            }

            TMP_Text[] textsProducto = producto.GetComponentsInChildren<TMP_Text>();

            if (textsProducto.Length < 3)
            {
                Debug.LogWarning($"[PrintTestFactura] Skipping row '{producto.name}', only {textsProducto.Length} TMP_Text children (need 3).");
                continue;
            }

            string cantidadRaw = Regex.Replace(textsProducto[2].text, @"[^\d\-]", "");
            if (!int.TryParse(cantidadRaw, out int cantidad))
            {
                Debug.LogWarning($"[PrintTestFactura] Skipping row '{producto.name}', cantidad not parseable: raw='{textsProducto[2].text}' cleaned='{cantidadRaw}'");
                continue;
            }

            string priceRaw = Regex.Replace(textsProducto[1].text, @"[^\d,.\-]", "").Replace(",", ".");
            decimal precio = decimal.TryParse(priceRaw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p : 0;
            Debug.Log($"[PrintTestFactura] row='{producto.name}' cantidadRaw='{textsProducto[2].text}'->{cantidad} priceRaw='{textsProducto[1].text}'->{precio}");

            string descripcion = textsProducto[0].text;

            productos.Add(new Producto
            {
                Cantidad = cantidad,
                Descripcion = descripcion,
                PrecioUnitario = precio
            });
        }


        string mesaDigitsFactura = Regex.Replace(numeroMesa.text, @"[^\d\-]", "");
        if (!int.TryParse(mesaDigitsFactura, out int mesaNumFactura))
        {
            Debug.LogError("[PrintTestFactura] numeroMesa.text no es un número válido: '" + numeroMesa.text + "'");
            return;
        }
        PrintFactura(mesaNumFactura, productos, nombreCamarero.text.Replace("Hola, ", ""), nombreRestaurante.text);
    }

    public void PrintTicket(int mesaNumber, List<Producto> productos, string nombreCamarero, string nombreRestaurante)
    {
        if (string.IsNullOrEmpty(defaultPrinter)) return;

        // === ESC/POS comandos ===
        byte[] init = new byte[] { 27, 64 };        // Inicializar
        byte[] boldOn = new byte[] { 27, 69, 1 };     // Negrita ON
        byte[] boldOff = new byte[] { 27, 69, 0 };     // Negrita OFF
        byte[] center = new byte[] { 27, 97, 1 };     // Centrado
        byte[] left = new byte[] { 27, 97, 0 };     // Izquierda
        byte[] doubleOff = new byte[] { 29, 33, 0 };     // Doble tama�o OFF
        byte[] feedLines = new byte[] { 27, 100, 2 };    // ESC d 5 -> avanzar 5 l�neas antes del corte
        byte[] cut = new byte[] { 29, 86, 1 };     // GS V 1 -> corte parcial (compat. com�n)

        // Usamos un buffer din�mico para concatenar bytes de forma segura
        var buf = new List<byte>();
        void Append(params byte[] bytes) { if (bytes != null) buf.AddRange(bytes); }
        void AppendText(string s) { if (s != null) buf.AddRange(Cp858.GetBytes(s)); }

        // ==== INICIALIZAR IMPRESORA ====
        Append(init);
        byte[] codePage858 = new byte[] { 27, 116, 19 };
        Append(codePage858);

        // ==== MARGEN IZQUIERDO GLOBAL ====
        byte[] setLeftMargin = new byte[] { 29, 76, 10, 0 }; // 10 columnas de margen
        Append(setLeftMargin);

        // ==== TITULO: centrado, negrita y doble tama�o ====
        Append(center);
        bool nameIsLong = RemoveAccents(nombreRestaurante).Length > 21;
        byte[] titleSize = nameIsLong ? new byte[] { 29, 33, 1 } : new byte[] { 29, 33, 17 };
        Append(boldOn);
        Append(titleSize);
        AppendText(RemoveAccents(nombreRestaurante) + "\r\n");
        Append(doubleOff);
        Append(boldOff);
        Append(left); // volver a izquierda para detalles

        // ==== CABECERA DETALLES ====
        var headerSb = new StringBuilder();
        headerSb.AppendLine("----------------------------------------");
        headerSb.AppendLine("Mesa: " + mesaNumber);
        headerSb.AppendLine("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        headerSb.AppendLine("----------------------------------------");
        AppendText(headerSb.ToString());

        // ==== ENCABEZADO PRODUCTOS EN NEGRITA ====
        string encabezado =
            "Uds".PadRight(4) +
            "Descripcion".PadRight(20) +
            "PVP".PadLeft(6) +
            "Importe".PadLeft(10) + "\r\n";
        Append(boldOn);
        AppendText(encabezado);
        Append(boldOff);

        // ==== PRODUCTOS ====
        decimal total = 0;
        var itemsSb = new StringBuilder();

        foreach (var p in productos)
        {
            decimal importe = p.Cantidad * p.PrecioUnitario;
            total += importe;

            itemsSb.AppendLine(
                p.Cantidad.ToString().PadRight(4) +
                Truncate(RemoveAccents(p.Descripcion), 19).PadRight(20) +
                (p.PrecioUnitario.ToString("0.00") + "\u20AC").PadLeft(6) +
                (importe.ToString("0.00") + "\u20AC").PadLeft(10)
            );
        }

        // IVA rate (10%)
        decimal ivaRate = 0.10m;
        decimal subtotal = total / (1 + ivaRate);
        decimal iva = total - subtotal;

        itemsSb.AppendLine("----------------------------------------");
        itemsSb.AppendLine("Subtotal:".PadLeft(30) + (subtotal.ToString("0.00") + "\u20AC").PadLeft(10));
        itemsSb.AppendLine("IVA (10%):".PadLeft(30) + (iva.ToString("0.00") + "\u20AC").PadLeft(10));
        itemsSb.AppendLine("TOTAL:".PadLeft(30) + (total.ToString("0.00") + "\u20AC").PadLeft(10));
        itemsSb.AppendLine("----------------------------------------");

        AppendText(itemsSb.ToString());

        // ==== AVANZAR UN POCO PARA ASEGURAR ESPACIO ANTES DEL MENSAJE FINAL ====
        Append(feedLines); // alimenta 5 l�neas

        // ==== MENSAJE FINAL: centrado (usar ESC a 1) y CRLFs correctos ====
        Append(center);
        AppendText("Gracias por su visita\r\n");
        AppendText("\r\n");
        AppendText("Atendido por: " + RemoveAccents(nombreCamarero) + "\r\n");
        AppendText("\r\n");
        AppendText("\r\n\r\n"); // espacio extra para que salga por completo

        // Volvemos a izquierda (opcional)
        Append(left);

        // ==== CORTE (final) ====
        Append(cut);

        // ==== ENVIAR A IMPRESORA ====
        SendBytesToPrinterSerial(buf.ToArray());
        Debug.Log("Ticket enviado correctamente.");
    }

    public void PrintFactura(int mesaNumber, List<Producto> productos, string nombreCamarero, string nombreRestaurante)
    {
        if (string.IsNullOrEmpty(defaultPrinter)) return;

        // === ESC/POS comandos ===
        byte[] init = new byte[] { 27, 64 };        // Inicializar
        byte[] boldOn = new byte[] { 27, 69, 1 };     // Negrita ON
        byte[] boldOff = new byte[] { 27, 69, 0 };     // Negrita OFF
        byte[] center = new byte[] { 27, 97, 1 };     // Centrado
        byte[] left = new byte[] { 27, 97, 0 };     // Izquierda
        byte[] doubleOff = new byte[] { 29, 33, 0 };     // Doble tama�o OFF
        byte[] feedLines = new byte[] { 27, 100, 2 };    // ESC d 5 -> avanzar 5 l�neas antes del corte
        byte[] cut = new byte[] { 29, 86, 1 };     // GS V 1 -> corte parcial (compat. com�n)

        // Usamos un buffer din�mico para concatenar bytes de forma segura
        var buf = new List<byte>();
        void Append(params byte[] bytes) { if (bytes != null) buf.AddRange(bytes); }
        void AppendText(string s) { if (s != null) buf.AddRange(Cp858.GetBytes(s)); }

        // ==== INICIALIZAR IMPRESORA ====
        Append(init);
        byte[] codePage858 = new byte[] { 27, 116, 19 };
        Append(codePage858);

        // ==== MARGEN IZQUIERDO GLOBAL ====
        byte[] setLeftMargin = new byte[] { 29, 76, 10, 0 }; // 10 columnas de margen
        Append(setLeftMargin);

        // ==== TITULO: centrado, negrita y doble tama�o ====
        Append(center);
        bool nameIsLong = RemoveAccents(nombreRestaurante).Length > 21;
        byte[] titleSize = nameIsLong ? new byte[] { 29, 33, 1 } : new byte[] { 29, 33, 17 };
        Append(boldOn);
        Append(titleSize);
        AppendText(RemoveAccents(nombreRestaurante) + "\r\n");
        Append(doubleOff);
        Append(boldOff);
        Append(left); // volver a izquierda para detalles

        // ==== CABECERA DETALLES ====
        var headerSb = new StringBuilder();
        headerSb.AppendLine("----------------------------------------");
        headerSb.AppendLine("Factura: " + numeroFactura.text);
        headerSb.AppendLine("NIF: " + NIFCliente.text);
        headerSb.AppendLine("Nombre: " + nombreCliente.text);
        headerSb.AppendLine("Direccion: " + direccionCliente.text);
        headerSb.AppendLine("Mesa: " + mesaNumber);
        headerSb.AppendLine("Pedido: " + numeroPedido.text);
        headerSb.AppendLine("Fecha expedicion: " + fechaExpedicion.text);
        headerSb.AppendLine("Fecha operacion: " + fechaOperacion.text);
        headerSb.AppendLine("----------------------------------------");
        AppendText(headerSb.ToString());

        // ==== ENCABEZADO PRODUCTOS EN NEGRITA ====
        string encabezado =
            "Uds".PadRight(4) +
            "Descripcion".PadRight(20) +
            "PVP".PadLeft(6) +
            "Importe".PadLeft(10) + "\r\n";
        Append(boldOn);
        AppendText(encabezado);
        Append(boldOff);

        // ==== PRODUCTOS ====
        decimal total = 0;
        var itemsSb = new StringBuilder();

        foreach (var p in productos)
        {
            decimal importe = p.Cantidad * p.PrecioUnitario;
            total += importe;

            itemsSb.AppendLine(
                p.Cantidad.ToString().PadRight(4) +
                Truncate(RemoveAccents(p.Descripcion), 19).PadRight(20) +
                (p.PrecioUnitario.ToString("0.00") + "\u20AC").PadLeft(6) +
                (importe.ToString("0.00") + "\u20AC").PadLeft(10)
            );
        }

        // IVA rate (10%)
        decimal ivaRate = 0.10m;
        decimal subtotal = total / (1 + ivaRate);
        decimal iva = total - subtotal;

        itemsSb.AppendLine("----------------------------------------");
        itemsSb.AppendLine("Subtotal:".PadLeft(30) + (subtotal.ToString("0.00") + "\u20AC").PadLeft(11));
        itemsSb.AppendLine("IVA (10%):".PadLeft(30) + (iva.ToString("0.00") + "\u20AC").PadLeft(11));
        itemsSb.AppendLine("TOTAL:".PadLeft(30) + (total.ToString("0.00") + "\u20AC").PadLeft(11));
        itemsSb.AppendLine("----------------------------------------");

        AppendText(itemsSb.ToString());

        // ==== AVANZAR UN POCO PARA ASEGURAR ESPACIO ANTES DEL MENSAJE FINAL ====
        Append(feedLines); // alimenta 5 l�neas

        // ==== MENSAJE FINAL: centrado (usar ESC a 1) y CRLFs correctos ====
        Append(center);
        AppendText("Gracias por su visita\r\n");
        AppendText("\r\n");
        AppendText("Atendido por: " + RemoveAccents(nombreCamarero) + "\r\n");
        AppendText("\r\n");
        AppendText("\r\n\r\n"); // espacio extra para que salga por completo

        // Volvemos a izquierda (opcional)
        Append(left);

        // ==== CORTE (final) ====
        Append(cut);

        // ==== ENVIAR A IMPRESORA ====
        SendBytesToPrinterSerial(buf.ToArray());
        Debug.Log("Ticket enviado correctamente.");
    }

    // === ACORTAR TEXTO ===
    private string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    // === QUITAR TILDES ===
    private string RemoveAccents(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = text.Normalize(System.Text.NormalizationForm.FormD);
        char[] chars = text.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars);
    }



    public void OpenDrawer()
    {
        if (string.IsNullOrEmpty(defaultPrinter)) return;

        byte[] init = new byte[] { 27, 64 };
        byte[] kick = new byte[] { 27, 112, 0, 25, 250 };

        var buf = new List<byte>();
        buf.AddRange(init);
        buf.AddRange(kick);

        SendBytesToPrinterSerial(buf.ToArray());

        // Registrar apertura en auditoría
        StartCoroutine(PostAuditoria());
    }

    private System.Collections.IEnumerator PostAuditoria()
    {
        string empleado = nombreCamarero.text.Replace("Hola, ", "");
        string id = restId.text;

        string json = "{\"id_rest\":\"" + id + "\",\"accion\":\"Apertura caja\",\"empleado\":\"" + empleado + "\"}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (var www = new UnityEngine.Networking.UnityWebRequest(
            "https://gastrali.tail634a78.ts.net/auditoria/add",
            "POST"))
        {
            www.uploadHandler   = new UnityEngine.Networking.UploadHandlerRaw(body);
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                Debug.LogWarning("Auditoria POST error: " + www.error);
            else
                Debug.Log("Auditoria registrada: " + www.downloadHandler.text);
        }
    }

    // P/Invoke to get default printer
    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int pcchBuffer);

    string GetDefaultPrinterName()
    {
        int length = 0;
        GetDefaultPrinter(null, ref length);
        if (length == 0) return null;

        var sb = new StringBuilder(length);
        return GetDefaultPrinter(sb, ref length) ? sb.ToString() : null;
    }

    public void PrintReporteX()
    {
        if (string.IsNullOrEmpty(defaultPrinter)) return;

        byte[] init = { 27, 64 };
        byte[] boldOn = { 27, 69, 1 };
        byte[] boldOff = { 27, 69, 0 };
        byte[] center = { 27, 97, 1 };
        byte[] left = { 27, 97, 0 };
        byte[] doubleOn = { 29, 33, 17 };
        byte[] doubleOff = { 29, 33, 0 };
        byte[] feedLines = { 27, 100, 2 };
        byte[] cut = { 29, 86, 1 };

        var buf = new List<byte>();
        void Append(params byte[] bytes) { buf.AddRange(bytes); }
        void AppendText(string s) { buf.AddRange(Cp858.GetBytes(s)); }

        Append(init);
        Append(new byte[] { 27, 116, 19 }); // code page 858
        Append(new byte[] { 29, 76, 10, 0 }); // left margin

        // Title
        bool nameIsLong = RemoveAccents(nombreRestaurante.text).Length > 21;
        byte[] titleSize = nameIsLong ? new byte[] { 29, 33, 1 } : new byte[] { 29, 33, 17 };
        Append(center); Append(boldOn); Append(titleSize);
        AppendText(RemoveAccents(nombreRestaurante.text) + "\r\n");
        Append(doubleOff); Append(boldOff);

        // Subtitle
        Append(boldOn);
        AppendText("REPORTE X\r\n");
        Append(boldOff);
        Append(left);

        var sb = new StringBuilder();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Fecha apertura:  " + reporteXFechaApertura.text);
        sb.AppendLine("Caja abierta por: " + reporteXAbiertoPor.text);
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Fondo de caja inicial:".PadRight(30) + reporteXFondoInicial.text.PadLeft(10));
        sb.AppendLine("Ingresos en efectivo:".PadRight(30) + reporteXIngresosEfectivo.text.PadLeft(10));
        sb.AppendLine("Ingresos por tarjeta:".PadRight(30) + reporteXIngresosTarjeta.text.PadLeft(10));
        sb.AppendLine("Depositos en caja:".PadRight(30) + reporteXDepositos.text.PadLeft(10));
        sb.AppendLine("Retiros de caja:".PadRight(30) + reporteXRetiros.text.PadLeft(10));
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Saldo en efectivo actual:".PadRight(30) + reporteXEfectivoActual.text.PadLeft(10));
        sb.AppendLine("----------------------------------------");
        AppendText(sb.ToString());

        Append(feedLines);

        // ==== MENSAJE FINAL: centrado (usar ESC a 1) y CRLFs correctos ====
        Append(center);
        AppendText("Impreso por: " + RemoveAccents(nombreCamarero.text.Replace("Hola, ", "")) + "\r\n");
        AppendText("\r\n");
        AppendText("\r\n\r\n"); // espacio extra para que salga por completo

        // Volvemos a izquierda (opcional)
        Append(left);

        // ==== CORTE (final) ====
        Append(cut);

        SendBytesToPrinterSerial(buf.ToArray());
        Debug.Log("Reporte X impreso.");
    }

    public void PrintReporteZ()
    {
        if (string.IsNullOrEmpty(defaultPrinter)) return;

        byte[] init = { 27, 64 };
        byte[] boldOn = { 27, 69, 1 };
        byte[] boldOff = { 27, 69, 0 };
        byte[] center = { 27, 97, 1 };
        byte[] left = { 27, 97, 0 };
        byte[] doubleOn = { 29, 33, 17 };
        byte[] doubleOff = { 29, 33, 0 };
        byte[] feedLines = { 27, 100, 2 };
        byte[] cut = { 29, 86, 1 };

        var buf = new List<byte>();
        void Append(params byte[] bytes) { buf.AddRange(bytes); }
        void AppendText(string s) { buf.AddRange(Cp858.GetBytes(s)); }

        Append(init);
        Append(new byte[] { 27, 116, 19 }); // code page 858
        Append(new byte[] { 29, 76, 10, 0 }); // left margin

        // Title
        bool nameIsLong = RemoveAccents(nombreRestaurante.text).Length > 21;
        byte[] titleSize = nameIsLong ? new byte[] { 29, 33, 1 } : new byte[] { 29, 33, 17 };
        Append(center); Append(boldOn); Append(titleSize);
        AppendText(RemoveAccents(nombreRestaurante.text) + "\r\n");
        Append(doubleOff); Append(boldOff);

        // Subtitle
        Append(boldOn);
        AppendText("REPORTE Z\r\n");
        Append(boldOff);
        Append(left);

        var sb = new StringBuilder();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Fecha apertura:   " + reporteZFechaApertura.text);
        sb.AppendLine("Abierta por:      " + reporteZAbiertoPor.text);
        sb.AppendLine("Fecha cierre:     " + reporteZFechaCierre.text);
        sb.AppendLine("Cerrada por:      " + reporteZCerradoPor.text);
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Fondo de caja inicial:".PadRight(30) + reporteZFondoInicial.text.PadLeft(10));
        sb.AppendLine("Ingresos en efectivo:".PadRight(30) + reporteZIngresosEfectivo.text.PadLeft(10));
        sb.AppendLine("Ingresos por tarjeta:".PadRight(30) + reporteZIngresosTarjeta.text.PadLeft(10));
        sb.AppendLine("Depositos en caja:".PadRight(30) + reporteZDepositos.text.PadLeft(10));
        sb.AppendLine("Retiros de caja:".PadRight(30) + reporteZRetiros.text.PadLeft(10));
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Saldo en efectivo actual:".PadRight(30) + reporteZEfectivoActual.text.PadLeft(10));
        sb.AppendLine("Fondo caja inicial teorico:".PadRight(30) + reporteZFinalTeorico.text.PadLeft(10));
        sb.AppendLine("Fondo caja inicial real:".PadRight(30) + reporteZFinalReal.text.PadLeft(10));
        sb.AppendLine("Descuadre:".PadRight(30) + reporteZDescuadre.text.PadLeft(10));
        sb.AppendLine("----------------------------------------");
        AppendText(sb.ToString());

        Append(feedLines);

        // ==== MENSAJE FINAL: centrado (usar ESC a 1) y CRLFs correctos ====
        Append(center);
        AppendText("Impreso por: " + RemoveAccents(nombreCamarero.text.Replace("Hola, ", "")) + "\r\n");
        AppendText("\r\n");
        AppendText("\r\n\r\n"); // espacio extra para que salga por completo

        // Volvemos a izquierda (opcional)
        Append(left);

        // ==== CORTE (final) ====
        Append(cut);

        SendBytesToPrinterSerial(buf.ToArray());
        Debug.Log("Reporte Z impreso.");
    }

    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA pDocInfo);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string printerName, byte[] data)
        {
            if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                Debug.LogError("OpenPrinter FAILED for '" + printerName + "'. Win32Error=" + Marshal.GetLastWin32Error());
                return false;
            }

            var docInfo = new DOCINFOA
            {
                pDocName  = "RAW_PRINT_JOB",
                pDataType = "RAW"
            };

            if (!StartDocPrinter(hPrinter, 1, docInfo))
            {
                Debug.LogError("StartDocPrinter FAILED. Win32Error=" + Marshal.GetLastWin32Error());
                ClosePrinter(hPrinter);
                return false;
            }

            if (!StartPagePrinter(hPrinter))
                Debug.LogError("StartPagePrinter FAILED. Win32Error=" + Marshal.GetLastWin32Error());

            IntPtr unmanagedBytes = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, unmanagedBytes, data.Length);
            bool writeOk = WritePrinter(hPrinter, unmanagedBytes, data.Length, out int written);
            if (!writeOk)
                Debug.LogError("WritePrinter FAILED. Win32Error=" + Marshal.GetLastWin32Error());
            else
                Debug.Log("WritePrinter OK. Bytes sent=" + written + " / " + data.Length);
            Marshal.FreeCoTaskMem(unmanagedBytes);

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            ClosePrinter(hPrinter);
            return writeOk;
        }
    }

    public void PrintTicketParcial(int mesaNumber, List<Producto> productos)
    {
        if (string.IsNullOrEmpty(defaultPrinter)) return;

        PrintTicket(mesaNumber, productos, nombreCamarero.text.Replace("Hola, ", ""), nombreRestaurante.text);
    }
}
