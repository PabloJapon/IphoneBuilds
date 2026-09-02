using UnityEngine;
using TMPro;

public class ReservasBadgeUI : MonoBehaviour
{
    public GameObject badgeContainer;
    public TMP_Text badgeCountText;

    void OnEnable()
    {
        ReservasBadgeManager.OnPendientesCountChanged += ActualizarBadge;

        if (ReservasBadgeManager.instance != null)
        {
            ActualizarBadge(ReservasBadgeManager.instance.ObtenerUltimoConteo());
        }
        else
        {
            Debug.LogWarning("[ReservasBadgeUI] ReservasBadgeManager.instance is NULL — is it in the scene?");
        }
    }

    void OnDisable()
    {
        ReservasBadgeManager.OnPendientesCountChanged -= ActualizarBadge;
    }

    void ActualizarBadge(int conteo)
    {
        if (badgeContainer == null || badgeCountText == null)
        {
            Debug.LogError("[ReservasBadgeUI] badgeContainer or badgeCountText not assigned in Inspector!");
            return;
        }

        if (conteo > 0)
        {
            badgeContainer.SetActive(true);
            badgeCountText.text = conteo.ToString();
        }
        else
        {
            badgeContainer.SetActive(false);
        }
    }
}