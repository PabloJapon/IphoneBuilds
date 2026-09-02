using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class PedidosAnterioresPanel : MonoBehaviour
{
    public MenuPedir MP;

    private UIDocument doc;
    private ScrollView scroll;

    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        scroll = root.Q<ScrollView>("pedidos-scroll");
        root.Q<Button>("pedidos-close").clicked += () => gameObject.SetActive(false);

        Refrescar();
    }

    public void Refrescar()
    {
        if (scroll == null) return;
        scroll.Clear();

        int mesaNumber;
        if (!int.TryParse(GameObject.FindGameObjectWithTag("inputMesa")?.GetComponent<TMPro.TMP_Text>()?.text, out mesaNumber))
            return;

        var orders = TPV_DataManager.instance != null
            ? TPV_DataManager.instance.GetOrdersForMesa(mesaNumber)
            : new List<TPV_DataManager.Order>();

        if (orders.Count == 0)
        {
            var empty = new Label("Sin pedidos anteriores.");
            empty.AddToClassList("pedidos-empty");
            scroll.Add(empty);
            return;
        }

        foreach (var order in orders)
            scroll.Add(CrearTarjetaPedido(order));
    }

    private VisualElement CrearTarjetaPedido(TPV_DataManager.Order order)
    {
        var card = new VisualElement();
        card.AddToClassList("pedido-card");

        var header = new VisualElement();
        header.AddToClassList("pedido-card-header");

        if (!string.IsNullOrWhiteSpace(order.tipo))
        {
            var badge = new Label(order.tipo);
            badge.AddToClassList("pedido-tipo-badge");
            header.Add(badge);
        }

        var fecha = new Label(order.date);
        fecha.AddToClassList("pedido-fecha");
        header.Add(fecha);

        card.Add(header);

        var itemsBox = new VisualElement();
        itemsBox.AddToClassList("pedido-items");

        foreach (var item in order.items)
        {
            var row = new VisualElement();
            row.AddToClassList("pedido-item-row");

            var cantidad = new Label(item.cantidad + "x");
            cantidad.AddToClassList("pedido-item-cantidad");
            row.Add(cantidad);

            string texto = item.nombre + (string.IsNullOrWhiteSpace(item.opciones) ? "" : $" ({item.opciones})");
            var nombre = new Label(texto);
            nombre.AddToClassList("pedido-item-nombre");
            row.Add(nombre);

            itemsBox.Add(row);
        }

        card.Add(itemsBox);

        var selBtn = new Button(() =>
        {
            MP?.AgregarPedidoAnterior(order.items);
            gameObject.SetActive(false);
        }) { text = "Seleccionar" };
        selBtn.AddToClassList("pedido-seleccionar-btn");
        card.Add(selBtn);

        return card;
    }
}