using UnityEngine;

public class TPVNotificaciones : MonoBehaviour
{
    public static TPVNotificaciones instance;

    public GameObject badgeMesas;
    public GameObject badgeRecoger;
    public GameObject badgeDelivery;

    private string currentTab = "Mesas"; // pestaña visible al iniciar TPV

    void Awake()
    {
        instance = this;
    }

    // Llamado desde MesaColorSync cuando una mesa cambia de color de verdad
    public void NotifyMesaChanged(int mesaNumber)
    {
        string seccion = SeccionDeMesa(mesaNumber);

        if (seccion == currentTab) return; // ya la estamos viendo, no hace falta avisar

        GameObject badge = BadgeDeSeccion(seccion);
        if (badge != null)
            badge.SetActive(true);
    }

    // Llamado desde el OnClick de cada botón de pestaña (Mesas/Recoger/Delivery)
    public void SelectTab(string seccion)
    {
        currentTab = seccion;

        GameObject badge = BadgeDeSeccion(seccion);
        if (badge != null)
            badge.SetActive(false);
    }

    private string SeccionDeMesa(int mesaNumber)
    {
        if (mesaNumber >= 1000 && mesaNumber < 2000) return "Recoger";
        if (mesaNumber >= 2000) return "Delivery";
        return "Mesas";
    }

    private GameObject BadgeDeSeccion(string seccion)
    {
        switch (seccion)
        {
            case "Mesas": return badgeMesas;
            case "Recoger": return badgeRecoger;
            case "Delivery": return badgeDelivery;
            default: return null;
        }
    }
}