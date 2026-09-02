using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlatoToggleDisponible : MonoBehaviour
{
    public Toggle toggle;
    public DataBase DB; // arrastra aquí el mismo objeto DataBase de la escena

    private int platoIndex; // índice en los arrays de DataBase (empieza en 0)

    public void Setup(int index, bool disponible)
    {
        platoIndex = index;

        toggle.SetIsOnWithoutNotify(disponible);

        SwitchToggleTPV switchVisual = toggle.GetComponent<SwitchToggleTPV>();
        if (switchVisual != null)
            switchVisual.RefreshVisual();

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private async void OnToggleChanged(bool nuevoValor)
    {
        toggle.interactable = false; // evita doble-click mientras se guarda

        await MenuDisponibilidadAPI.ActualizarDisponibilidad(DB.url, platoIndex, nuevoValor);

        toggle.interactable = true;
    }
}