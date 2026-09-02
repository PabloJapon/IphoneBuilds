using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;

public class TPV_DataManager : MonoBehaviour
{
    [System.Serializable]
    public class Customer
    {
        public int id;
        public string name;
        public string address;
        public string phoneNumber;
        public string nif;
    }

    [System.Serializable]
    public class InvoiceItem
    {
        public string description;
        public int quantity;
        public float unitPrice;
        public float taxRate;
    }

    [System.Serializable]
    public class Invoice
    {
        public string id;
        public string date;
        public int customerId;
        public List<InvoiceItem> items = new List<InvoiceItem>();
        public float subtotal;
        public float taxAmount;
        public float total;
        public bool isFinal;
        public string hash;
        public bool verifactuEnabled;
        public bool verifactuSent;
        public string verifactuQR;
    }

    [System.Serializable]
    public class InvoiceAudit
    {
        public string invoiceId;
        public string action;
        public string timestamp;
        public string user;
        public string hash;
    }

    [System.Serializable]
    public class OrderItem
    {
        public string nombre;
        public string opciones;
        public string cantidad;
        public string precio;
    }

    [System.Serializable]
    public class Order
    {
        public int customerId;
        public string date;
        public int mesaNumber;
        public string tipo;
        public List<OrderItem> items = new List<OrderItem>();
    }

    [System.Serializable]
    public class TPVData
    {
        public List<Customer> customers = new List<Customer>();
        public List<Invoice> invoices = new List<Invoice>();
        public List<InvoiceAudit> invoiceAuditLog = new List<InvoiceAudit>();
        public List<Order> orderHistory = new List<Order>();
    }

    private string filePath;
    public TPVData tpvData = new TPVData();

    public static TPV_DataManager instance;
    public static Dictionary<int, int> mesaCustomerMap = new Dictionary<int, int>();
    public static Dictionary<int, string> mesaTipoMap = new Dictionary<int, string>();

    // =========================================================
    // PEDIDOS / RECOGIDAS
    // =========================================================

    private Customer selectedCustomerToUpdate;

    public TMP_InputField inputFieldNombre;
    public TMP_InputField inputFieldTelefono;
    public TMP_InputField inputFieldDireccion;

    public GameObject suggestionPanel;
    public GameObject suggestionPrefab;

    public GameObject guardarCliente;
    public GameObject actualizarCliente;

    // =========================================================
    // FACTURAS
    // =========================================================

    private Customer selectedCustomerToUpdate2;

    public TMP_InputField inputFieldNombre2;
    public TMP_InputField inputFieldNif2;
    public TMP_InputField inputFieldDireccion2;

    public GameObject suggestionPanel2;

    public GameObject guardarCliente2;
    public GameObject actualizarCliente2;

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    void Start()
    {
        instance = this;

        filePath = Path.Combine(Application.persistentDataPath, "TPV_database.json");

        LoadData();

        guardarCliente.SetActive(false);
        actualizarCliente.SetActive(false);

        guardarCliente2.SetActive(false);
        actualizarCliente2.SetActive(false);
    }

    // =========================================================
    // SHARED SUGGESTION LOGIC
    // =========================================================

    private void HandleNameInputChanged(
        string input,
        GameObject suggestionPanel,
        GameObject guardarBtn,
        bool showNif,
        System.Action<Customer> onSuggestionSelected)
    {
        guardarBtn.SetActive(true);
        SetToggleState(guardarBtn, true);

        ClearSuggestions(suggestionPanel);

        if (input.Length > 0)
        {
            suggestionPanel.SetActive(true);

            List<Customer> filtered = tpvData.customers.FindAll(c =>
                !string.IsNullOrEmpty(c.name) &&
                c.name.ToLower().Contains(input.ToLower().Trim()));

            foreach (var customer in filtered)
            {
                GameObject btn = Instantiate(suggestionPrefab, suggestionPanel.transform);
                TextMeshProUGUI text = btn.GetComponentInChildren<TextMeshProUGUI>();

                text.text = showNif
                    ? $"{customer.name}, {customer.nif}, {customer.address}"
                    : $"{customer.name}, {customer.phoneNumber}, {customer.address}";

                btn.GetComponent<Button>().onClick.AddListener(() => onSuggestionSelected(customer));
            }
        }
        else
        {
            suggestionPanel.SetActive(false);
        }
    }

    // =========================================================
    // PEDIDOS / RECOGIDAS
    // =========================================================

    public void OnNameInputChanged()
    {
        HandleNameInputChanged(
            inputFieldNombre.text,
            suggestionPanel,
            guardarCliente,
            showNif: false,
            onSuggestionSelected: customer =>
            {
                inputFieldNombre.text = customer.name;
                inputFieldDireccion.text = customer.address;
                inputFieldTelefono.text = customer.phoneNumber;

                suggestionPanel.SetActive(false);
                actualizarCliente.SetActive(true);
                guardarCliente.SetActive(false);
                selectedCustomerToUpdate = customer;
            });
    }

    public void AddCustomer()
    {
        string name    = inputFieldNombre.text.Trim();
        string address = inputFieldDireccion.text.Trim();
        string phone   = inputFieldTelefono.text.Trim();

        if (guardarCliente.GetComponent<Toggle>().isOn)
        {
            // Check if customer with same name already exists
            Customer existing = tpvData.customers.Find(c =>
                string.Equals(c.name, name, System.StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // Update instead of duplicating
                existing.address     = address;
                existing.phoneNumber = phone;
            }
            else
            {
                tpvData.customers.Add(new Customer
                {
                    id          = GetNextCustomerId(),
                    name        = name,
                    address     = address,
                    phoneNumber = phone
                });
            }
            SaveData();
        }

        if (actualizarCliente.activeInHierarchy &&
            actualizarCliente.GetComponent<Toggle>().isOn &&
            selectedCustomerToUpdate != null)
        {
            selectedCustomerToUpdate.address     = address;
            selectedCustomerToUpdate.phoneNumber = phone;
            SaveData();
        }

        ClearCustomerFields();
    }


    public void PrefillFromPhoneNumber(string numero)
    {
        inputFieldTelefono.text = numero;

        Customer existing = tpvData.customers.Find(c => c.phoneNumber == numero);
        if (existing != null)
        {
            inputFieldNombre.text = existing.name;
            inputFieldDireccion.text = existing.address;
        }
    }

    public void Cancelar() => ClearCustomerFields();

    // =========================================================
    // FACTURAS
    // =========================================================

    public void OnNameInputChanged2()
    {
        HandleNameInputChanged(
            inputFieldNombre2.text,
            suggestionPanel2,
            guardarCliente2,
            showNif: true,
            onSuggestionSelected: customer =>
            {
                inputFieldNombre2.text = customer.name;
                inputFieldDireccion2.text = customer.address;
                inputFieldNif2.text = customer.nif;

                suggestionPanel2.SetActive(false);
                actualizarCliente2.SetActive(true);
                guardarCliente2.SetActive(false);
                selectedCustomerToUpdate2 = customer;
            });
    }

    public void AddCustomer2()
    {
        string name    = inputFieldNombre2.text.Trim();
        string address = inputFieldDireccion2.text.Trim();
        string nif     = inputFieldNif2.text.Trim();

        if (guardarCliente2.activeInHierarchy && guardarCliente2.GetComponent<Toggle>().isOn)
        {
            // Check if customer with same name already exists
            Customer existing = tpvData.customers.Find(c =>
                string.Equals(c.name, name, System.StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // Update NIF and address on the existing record
                existing.address = address;
                existing.nif     = nif;
            }
            else
            {
                tpvData.customers.Add(new Customer
                {
                    id      = GetNextCustomerId(),
                    name    = name,
                    address = address,
                    nif     = nif
                });
            }
            SaveData();
        }

        if (actualizarCliente2.activeInHierarchy &&
            actualizarCliente2.GetComponent<Toggle>().isOn &&
            selectedCustomerToUpdate2 != null)
        {
            selectedCustomerToUpdate2.address = address;
            selectedCustomerToUpdate2.nif     = nif;
            SaveData();
        }

        ClearCustomerFields2();
    }

    public void Cancelar2() => ClearCustomerFields2();

    // =========================================================
    // ORDER HISTORY
    // =========================================================

    public int GetOrCreateCustomerId(string name)
    {
        name = name.Trim();
        Customer existing = tpvData.customers.Find(c =>
            !string.IsNullOrEmpty(c.name) &&
            string.Equals(c.name, name, System.StringComparison.OrdinalIgnoreCase));

        if (existing != null)
            return existing.id;

        Customer newCustomer = new Customer { id = GetNextCustomerId(), name = name };
        tpvData.customers.Add(newCustomer);
        SaveData();
        return newCustomer.id;
    }

    public List<Order> GetOrdersForMesa(int mesaNumber)
    {
        if (!mesaCustomerMap.TryGetValue(mesaNumber, out int customerId))
            return new List<Order>();

        return tpvData.orderHistory
            .Where(o => o.customerId == customerId)
            .OrderByDescending(o => o.date)
            .ToList();
    }

    public void SaveOrderToHistory(int customerId, int mesaNumber, string tipo, string[] nombres, string[] opciones, string[] cantidades, string[] precios)
    {
        Order order = new Order
        {
            customerId = customerId,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            mesaNumber = mesaNumber,
            tipo = tipo
        };

        for (int i = 0; i < nombres.Length; i++)
        {
            order.items.Add(new OrderItem
            {
                nombre = nombres[i],
                opciones = opciones[i],
                cantidad = cantidades[i],
                precio = precios[i]
            });
        }

        tpvData.orderHistory.Add(order);
        SaveData();
    }

    // =========================================================
    // COMMON METHODS
    // =========================================================

    int GetNextCustomerId()
    {
        return tpvData.customers.Count > 0
            ? tpvData.customers.Max(c => c.id) + 1
            : 1;
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(tpvData, true);
            File.WriteAllText(filePath, json);
            Debug.Log("Data saved to: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving data: " + e.Message);
        }
    }

    void LoadData()
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                    tpvData = JsonUtility.FromJson<TPVData>(json);

                if (tpvData == null)
                    tpvData = new TPVData();
            }
            else
            {
                Debug.Log("No database found. Creating new database.");
                tpvData = new TPVData();
                SaveData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading data: " + e.Message);
            tpvData = new TPVData();
        }
    }

    void ClearSuggestions(GameObject panel)
    {
        foreach (Transform child in panel.transform)
            Destroy(child.gameObject);
    }

    void SetToggleState(GameObject parent, bool state)
    {
        foreach (Toggle toggle in parent.GetComponentsInChildren<Toggle>())
            toggle.isOn = state;
    }

    void ClearCustomerFields()
    {
        inputFieldNombre.text = "";
        inputFieldDireccion.text = "";
        inputFieldTelefono.text = "";

        suggestionPanel.SetActive(false);
        guardarCliente.SetActive(false);
        actualizarCliente.SetActive(false);

        selectedCustomerToUpdate = null;

        SetToggleState(guardarCliente, false);
        SetToggleState(actualizarCliente, false);
    }

    void ClearCustomerFields2()
    {
        inputFieldNombre2.text = "";
        inputFieldDireccion2.text = "";
        inputFieldNif2.text = "";

        suggestionPanel2.SetActive(false);
        guardarCliente2.SetActive(false);
        actualizarCliente2.SetActive(false);

        selectedCustomerToUpdate2 = null;

        SetToggleState(guardarCliente2, false);
        SetToggleState(actualizarCliente2, false);
    }

    // =========================================================
    // INVOICE HASHING / AUDIT
    // =========================================================

    string CalculateInvoiceHash(Invoice invoice)
    {
        Invoice clone = new Invoice
        {
            id = invoice.id,
            date = invoice.date,
            customerId = invoice.customerId,
            items = invoice.items,
            subtotal = invoice.subtotal,
            taxAmount = invoice.taxAmount,
            total = invoice.total,
            isFinal = invoice.isFinal,
            verifactuEnabled = invoice.verifactuEnabled,
            verifactuSent = invoice.verifactuSent,
            verifactuQR = invoice.verifactuQR
        };

        string json = JsonUtility.ToJson(clone);

        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] hashBytes = sha.ComputeHash(bytes);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public void FinalizeInvoice(Invoice invoice, string user)
    {
        invoice.isFinal = true;
        invoice.hash = CalculateInvoiceHash(invoice);

        tpvData.invoiceAuditLog.Add(new InvoiceAudit
        {
            invoiceId = invoice.id,
            action = "created",
            timestamp = System.DateTime.UtcNow.ToString("o"),
            user = user,
            hash = invoice.hash
        });

        SaveData();
    }

    bool IsInvoiceValid(Invoice invoice)
    {
        return invoice.hash == CalculateInvoiceHash(invoice);
    }
}