using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mirror;

public class LongPressDebug : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float holdTime = 1.0f;
    public GameObject canvasToShow;
    public static GameObject selectionCanvas; // to store a shared canvas reference
    private Toggle selectionToggle;
    public static bool selectionModeActive = false;

    // Cambiar mesa
    public GameObject cambiarMesaCanvas;
    public TMP_InputField newMesaInput; 
    private float changingMesaId = -1;


    private bool isPointerDown = false;
    private float pointerDownTimer;

    // Seguro separar mesas
    public GameObject cuadroSeguroSepararMesas;
    public TMP_Text seguroSepararMesasText;

    // Group tracking: each list is a group of mesa IDs joined together
    public static List<List<float>> mesaGroups = new List<List<float>>();
    private static List<float> pendingSepararGroup = null; // group awaiting confirmation

    public TMP_Text cambiarMesaPreguntaText; // para adaptar el texto al número correspondiente

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        StartCoroutine(LongPressCheck());
    }

    public void OnPointerUp(PointerEventData eventData) { Reset(); }
    public void OnPointerExit(PointerEventData eventData) { Reset(); }

    private IEnumerator LongPressCheck()
    {
        pointerDownTimer = 0;

        while (isPointerDown)
        {
            pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= holdTime)
            {
                Debug.Log("🎯 Long press detected!");

                selectionModeActive = true;
                // Show canvas SeleccionMesas
                if (canvasToShow != null)
                {
                    canvasToShow.SetActive(true);
                    selectionCanvas = canvasToShow; // ✅ store for global access
                }

                // Activate toggles of all the buttons 
                ShowMesaToggles(true);

                // On toggle this button clicked


                Reset();
            }

            yield return null;
        }
    }

    public void OnClick()
    {
        if (!selectionModeActive)
            return;

        // Only toggle if in selection mode
        if (selectionToggle == null)
            selectionToggle = GetComponentInChildren<Toggle>(true);

        if (selectionToggle != null)
        {
            selectionToggle.isOn = !selectionToggle.isOn; // ✅ toggle selection
        }
    }


    public static void ShowMesaToggles(bool visible)
    {
        Color colorSec = GetColorSecundario();

        foreach (var pair in CrearCamarero.buttonMesaDictionary)
        {
            Toggle toggle = pair.Value.GetComponentInChildren<Toggle>(true);
            if (toggle != null)
            {
                toggle.gameObject.SetActive(visible);

                Transform checkmark = toggle.transform.Find("Background/Checkmark");
                if (checkmark != null)
                    checkmark.GetComponent<Image>().color = colorSec;
            }
        }
    }

    public void JuntarMesas()
    {
        List<float> selectedIds = new List<float>();

        foreach (var pair in CrearCamarero.buttonMesaDictionary)
        {
            Toggle toggle = pair.Value.GetComponentInChildren<Toggle>(true);
            if (toggle != null && toggle.isOn)
                selectedIds.Add(pair.Key);
        }

        if (selectedIds.Count == 0)
            return;

        // Merge any existing groups that contain selected IDs into one
        List<List<float>> groupsToMerge = new List<List<float>>();
        List<float> ungrouped = new List<float>();

        foreach (float id in selectedIds)
        {
            List<float> existingGroup = mesaGroups.Find(g => g.Contains(id));
            if (existingGroup != null)
            {
                if (!groupsToMerge.Contains(existingGroup))
                    groupsToMerge.Add(existingGroup);
            }
            else
            {
                ungrouped.Add(id);
            }
        }

        // Build the merged group
        List<float> newGroup = new List<float>();
        foreach (var g in groupsToMerge)
        {
            newGroup.AddRange(g);
            mesaGroups.Remove(g);
        }
        newGroup.AddRange(ungrouped);

        // Deduplicate just in case
        newGroup = new List<float>(new HashSet<float>(newGroup));
        mesaGroups.Add(newGroup);

        Debug.Log($"[JuntarMesas] New group registered: [{string.Join(", ", newGroup)}]");

        NetworkMesaHandler handler = NetworkClient.localPlayer.GetComponent<NetworkMesaHandler>();
        handler.CmdJuntarMesas(selectedIds);

        ClearMesaSelectionFromButton();
    }

    public void SepararMesas()
    {
        // Find the first selected mesa
        float selectedId = -1;

        foreach (var pair in CrearCamarero.buttonMesaDictionary)
        {
            Toggle toggle = pair.Value.GetComponentInChildren<Toggle>(true);
            if (toggle != null && toggle.isOn)
            {
                selectedId = pair.Key;
                break; // only need one to find its group
            }
        }

        if (selectedId < 0)
        {
            Debug.LogWarning("⚠️ No mesa selected.");
            return;
        }

        // Find the group this mesa belongs to
        List<float> group = mesaGroups.Find(g => g.Contains(selectedId));

        if (group == null || group.Count <= 1)
        {
            Debug.LogWarning($"⚠️ Mesa {selectedId} is not part of a group.");
            return;
        }

        // Store pending group and show confirmation
        pendingSepararGroup = group;

        // Build confirmation message e.g. "¿Quieres separar las mesas 14, 23 y 45?"
        string mesaList = FormatMesaList(group);
        if (seguroSepararMesasText != null)
            seguroSepararMesasText.text = $"¿Quieres separar las mesas {mesaList}?";

        if (cuadroSeguroSepararMesas != null)
            cuadroSeguroSepararMesas.SetActive(true);
    }

    public void CambiarMesa()
    {
        List<float> selectedIds = new List<float>();

        foreach (var pair in CrearCamarero.buttonMesaDictionary)
        {
            Toggle toggle = pair.Value.GetComponentInChildren<Toggle>(true);
            if (toggle != null && toggle.isOn)
            {
                selectedIds.Add(pair.Key);
            }
        }

        if (selectedIds.Count != 1)
        {
            Debug.LogWarning("⚠️ Cambiar Mesa requires exactly ONE mesa selected.");
            return;
        }
        // Show the input canvas
        cambiarMesaCanvas.SetActive(true);

        // Optionally, clear previous value
        newMesaInput.text = "";

        // Save the current mesa being changed
        changingMesaId = selectedIds[0];

        // Update question text
        if (cambiarMesaPreguntaText != null)
        {
            cambiarMesaPreguntaText.text =
                $"¿A qué mesa quieres cambiar las comandas de la mesa {changingMesaId}?";
        }
    }

    public void OnConfirmCambiarMesa()
    {
        if (changingMesaId < 0)
        {
            Debug.LogError("❌ No mesa selected to change.");
            return;
        }

        if (float.TryParse(newMesaInput.text, out float newId))
        {
            Debug.Log($"✅ Cambiar Mesa: {changingMesaId} ➡ {newId}");

            NetworkMesaHandler handler = NetworkClient.localPlayer.GetComponent<NetworkMesaHandler>();
            handler.CmdCambiarMesa(changingMesaId, newId);

            cambiarMesaCanvas.SetActive(false);
            changingMesaId = -1;
        }
        else
        {
            Debug.LogWarning("⚠️ Invalid input. Please enter a valid number.");
        }

        ClearMesaSelectionFromButton();
    }



    public void ClearMesaSelectionFromButton()
    {
        ClearMesaSelection(); // calls the static version
        if (selectionCanvas != null)
            selectionCanvas.SetActive(false); // 🛑 hide canvas too
    }


    public static void ClearMesaSelection()
    {
        selectionModeActive = false;

        foreach (var pair in CrearCamarero.buttonMesaDictionary)
        {
            Toggle toggle = pair.Value.GetComponentInChildren<Toggle>(true);
            if (toggle != null)
            {
                toggle.isOn = false; // ❌ uncheck it
                toggle.gameObject.SetActive(false); // 👻 hide the toggle
            }
        }
    }

    private void Reset()
    {
        isPointerDown = false;
        pointerDownTimer = 0;
    }
    public static void HandleJuntarMesasFromNetwork(float[] mesaIds)
    {
        if (mesaIds == null || mesaIds.Length == 0)
            return;

        float mainId = mesaIds[0];
        ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_empl[0], out Color colorSec);

        foreach (float id in mesaIds)
        {
            if (CrearCamarero.buttonMesaDictionary.TryGetValue(id, out GameObject btn))
            {
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = mainId.ToString();

                // Find ImageMesasLink by name instead of child index
                Transform linkImage = btn.transform.Find("ImageMesasLink");
                if (linkImage != null)
                {
                    linkImage.gameObject.SetActive(true);
                    linkImage.GetComponent<Image>().color = colorSec; // 👈 recuperado
                }
                else
                    Debug.LogWarning($"⚠️ ImageMesasLink not found in button for mesa {id}");

                if (id != mainId)
                    btn.SetActive(false);
            }
        }

        Debug.Log($"[HandleJuntarMesasFromNetwork] Mesas juntadas bajo ID: {mainId}, total: {mesaIds.Length}");
    }

    public static void HandleSepararMesasFromNetwork(float[] mesaIds)
    {
        foreach (float id in mesaIds)
        {
            if (CrearCamarero.buttonMesaDictionary.TryGetValue(id, out GameObject btn))
            {
                // 👇 Volver a mostrar el botón
                btn.SetActive(true);

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                    label.text = id.ToString();

                // Hide ImageMesasLink by name
                Transform linkImage = btn.transform.Find("ImageMesasLink");
                if (linkImage != null)
                    linkImage.gameObject.SetActive(false);
                else
                    Debug.LogWarning($"⚠️ ImageMesasLink not found in button for mesa {id}");

                Toggle toggle = btn.GetComponentInChildren<Toggle>(true);
                if (toggle != null)
                    toggle.isOn = false;
            }
        }

        Debug.Log($"[HandleSepararMesasFromNetwork] Mesas separadas: {mesaIds.Length}");
    }

    public static void HandleCambiarMesaFromNetwork(float oldId, float newId)
    {
        int idA = (int)oldId;
        int idB = (int)newId;

        if (!CrearCamarero.mesasDictionary.TryGetValue(idA, out GameObject mesaA) ||
            !CrearCamarero.mesasDictionary.TryGetValue(idB, out GameObject mesaB))
        {
            Debug.LogWarning($"❌ Could not find mesas {idA} and/or {idB}");
            return;
        }

        Transform contentA = mesaA.transform.Find("Scroll View/Viewport/Content");
        Transform contentB = mesaB.transform.Find("Scroll View/Viewport/Content");

        if (contentA == null || contentB == null)
        {
            Debug.LogWarning("❌ Content not found in one or both mesas");
            return;
        }

        // Swap children between contents
        List<Transform> childrenA = new List<Transform>();
        List<Transform> childrenB = new List<Transform>();

        foreach (Transform child in contentA) childrenA.Add(child);
        foreach (Transform child in contentB) childrenB.Add(child);

        foreach (Transform child in childrenA) child.SetParent(contentB, false);
        foreach (Transform child in childrenB) child.SetParent(contentA, false);

        // 🔄 Swap button colors
        if (CrearCamarero.buttonMesaDictionary.TryGetValue(idA, out GameObject buttonA) &&
            CrearCamarero.buttonMesaDictionary.TryGetValue(idB, out GameObject buttonB))
        {
            // 🟥 Swap Image colors
            Image imageA = buttonA.GetComponent<Image>();
            Image imageB = buttonB.GetComponent<Image>();

            if (imageA != null && imageB != null)
            {
                Color tempColor = imageA.color;
                imageA.color = imageB.color;
                imageB.color = tempColor;
            }

            // 🔤 Swap TMP_Text colors
            TMP_Text textA = buttonA.GetComponentInChildren<TMP_Text>(true);
            TMP_Text textB = buttonB.GetComponentInChildren<TMP_Text>(true);

            if (textA != null && textB != null)
            {
                Color tempTextColor = textA.color;
                textA.color = textB.color;
                textB.color = tempTextColor;
            }
        }

        CrearCamarero crearCamarero = Object.FindObjectOfType<CrearCamarero>();
        if (crearCamarero != null)
        {
            bool mesaAHasContent = contentA.childCount > 0;
            bool mesaBHasContent = contentB.childCount > 0;

            Debug.Log($"[CambiarMesa] idA={idA} childCount={contentA.childCount} hasContent={mesaAHasContent}");
            Debug.Log($"[CambiarMesa] idB={idB} childCount={contentB.childCount} hasContent={mesaBHasContent}");

            var buttonsA = CrearCamarero.mesasDictionary.TryGetValue(idA, out GameObject panelA)
                ? panelA.GetComponentsInChildren<Button>() : null;
            var buttonsB = CrearCamarero.mesasDictionary.TryGetValue(idB, out GameObject panelB)
                ? panelB.GetComponentsInChildren<Button>() : null;

            Debug.Log($"[CambiarMesa] idA buttons={(buttonsA != null ? buttonsA.Length : -1)}");
            Debug.Log($"[CambiarMesa] idB buttons={(buttonsB != null ? buttonsB.Length : -1)}");

            crearCamarero.SetMesaButtonsInteractable(idA, mesaAHasContent);
            crearCamarero.SetMesaButtonsInteractable(idB, mesaBHasContent);
        }
    }

    // Called by the "Sí" button on CuadroSeguroSepararMesas
    public void OnConfirmSepararMesas()
    {
        if (pendingSepararGroup == null || pendingSepararGroup.Count == 0)
        {
            Debug.LogWarning("⚠️ No pending group to separate.");
            return;
        }

        NetworkMesaHandler handler = NetworkClient.localPlayer.GetComponent<NetworkMesaHandler>();
        handler.CmdSepararMesas(pendingSepararGroup);

        // Remove the group from tracking
        mesaGroups.Remove(pendingSepararGroup);
        Debug.Log($"[SepararMesas] Group removed from tracking: [{string.Join(", ", pendingSepararGroup)}]");

        pendingSepararGroup = null;

        if (cuadroSeguroSepararMesas != null)
            cuadroSeguroSepararMesas.SetActive(false);

        ClearMesaSelectionFromButton();
    }

    // Called by the "No" button on CuadroSeguroSepararMesas
    public void OnCancelSepararMesas()
    {
        pendingSepararGroup = null;

        if (cuadroSeguroSepararMesas != null)
            cuadroSeguroSepararMesas.SetActive(false);
    }

    // Formats [14, 23, 45] → "14, 23 y 45"
    private string FormatMesaList(List<float> ids)
    {
        if (ids.Count == 1)
            return ids[0].ToString();

        List<string> parts = new List<string>();
        for (int i = 0; i < ids.Count; i++)
            parts.Add(ids[i].ToString());

        string last = parts[parts.Count - 1];
        parts.RemoveAt(parts.Count - 1);
        return string.Join(", ", parts) + " y " + last;
    }

    private static Color GetColorSecundario()
    {
        ColorUtility.TryParseHtmlString(DataBasePersonalizacion.col_sec_empl[0], out Color c);
        return c;
    }

}
