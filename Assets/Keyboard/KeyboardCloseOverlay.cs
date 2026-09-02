using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// Se pone en un panel invisible a pantalla completa, detrás del teclado.
// Reenvía el toque a lo que haya debajo (input field, botón, etc.) para que
// funcione con un solo toque. Solo cierra el teclado si lo tocado NO es un
// TMP_InputField (si lo es, es él quien decide mantenerlo abierto vía onSelect).
public class KeyboardCloseOverlay : MonoBehaviour, IPointerClickHandler
{
    public OnScreenKeyboardController keyboard;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (keyboard.WasShownThisFrame()) return;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        GameObject target = null;
        foreach (var result in results)
        {
            if (result.gameObject == gameObject) continue;
            target = result.gameObject;
            break; // el primero que no sea el propio overlay
        }

        bool esCampoDeTexto = target != null && target.GetComponentInParent<TMP_InputField>() != null;

        if (!esCampoDeTexto)
            keyboard.Hide();

        if (target != null)
        {
            eventData.clickCount = 1; 

            ExecuteEvents.ExecuteHierarchy(target, eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(target, eventData, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(target, eventData, ExecuteEvents.pointerClickHandler);
        }
    }
}