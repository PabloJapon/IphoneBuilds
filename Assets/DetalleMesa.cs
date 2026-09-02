using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;

public class DetalleMesa : MonoBehaviour
{
    public GameObject detalleMesa;
    public TMP_Text textDetalleM;

    public MenuPedir menuPedir;
    private GameObject[] mesas;
    private int childCount; // Declare childCount as a class-level variable

    public void Start()
    {
        // Initialize the array with child GameObjects
        childCount = detalleMesa.transform.childCount;
        mesas = new GameObject[childCount];

        StartCoroutine(WaitABit());
    }

    IEnumerator WaitABit()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 3; i < childCount; i++)
        {
            mesas[i] = detalleMesa.transform.GetChild(i).gameObject;
        }
    }

    public void click(float numeroMesa, int totalMesas)
    {
        for (int i = 0; i < totalMesas; i++)
        {
            RectTransform rt = mesas[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            // Move it 4000 units to the right (off-screen)
            rt.offsetMin += new Vector2(4000, 0);
            rt.offsetMax += new Vector2(4000, 0);
        }
        //detalleMesa.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 370f);   // Move it

        textDetalleM.text = "MESA " + numeroMesa;
        SetDetalleMesaTextAlpha(1f); // Asegurarnos de que no esté transparente, ya que lo hacemos invisible en CrearCamarero

        int index = Mathf.FloorToInt(numeroMesa) + 2; // Adjust index

        // Activate the corresponding mesa
        if (index >= 0 && index < mesas.Length)
        {
            RectTransform rt = mesas[index].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0); // Move it;
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, -500);
        }
        else
        {
            Debug.LogWarning("Mesa index out of range");
        }
    }

    public void clickClose()
    {
        if (CobrosCamarero.pagoConfirmadoEnCurso) return; // 👈 AÑADIR
        detalleMesa.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10000);
    }

    void SetDetalleMesaTextAlpha(float alpha)
    {
        Color c = textDetalleM.color;
        c.a = alpha;
        textDetalleM.color = c;
    }
}
