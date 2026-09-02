// MesaData.cs
using System;

[Serializable]
public class MesaData
{
    public int nEspacios;
    public string[] nombrePlatoString;
    public string[] opcionesPlato;
    public string[] cantidadPlatoString;
    public string[] precioPlatoString;
    public int[] togglePlato;
    public int[] estadoPlato;
    public string[] notaPlato;
    public int[] ordenPlato;
    public int[] batchIdPlato;

    public MesaData()
    {
        nEspacios = 0;
        nombrePlatoString = new string[0];
        opcionesPlato = new string[0];
        cantidadPlatoString = new string[0];
        precioPlatoString = new string[0];
        togglePlato = new int[0];
        estadoPlato = new int[0];
        notaPlato = new string[0];
        ordenPlato = new int[0];
        batchIdPlato = new int[0];
    }

    public MesaData(int nEspacios,
                    string[] nombrePlatoString,
                    string[] opcionesPlato,
                    string[] cantidadPlatoString,
                    string[] precioPlatoString,
                    int[] togglePlato,
                    string[] notaPlato = null,
                    int[] ordenPlato = null,
                    int[] batchIdPlato = null)
    {
        this.nEspacios = nEspacios;
        this.nombrePlatoString = nombrePlatoString;
        this.opcionesPlato = opcionesPlato;
        this.cantidadPlatoString = cantidadPlatoString;
        this.precioPlatoString = precioPlatoString;
        this.togglePlato = togglePlato;
        this.estadoPlato = new int[nEspacios];
        this.notaPlato = notaPlato ?? new string[nEspacios];
        this.ordenPlato = ordenPlato ?? new int[nEspacios];
        this.batchIdPlato = batchIdPlato ?? new int[nEspacios];
    }
}