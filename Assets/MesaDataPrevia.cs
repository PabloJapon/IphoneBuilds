using System;
[Serializable]
public class MesaDataPrevia
{
    public int[] ownerConnectionId;
    public string[] nombrePlatoString;
    public string[] opcionesPlato;
    public string[] cantidadPlatoString;
    public string[] precioPlatoString;
    public int[] togglePlato;

    public MesaDataPrevia()
    {
        ownerConnectionId = new int[0];
        nombrePlatoString = new string[0];
        opcionesPlato = new string[0];
        cantidadPlatoString = new string[0];
        precioPlatoString = new string[0];
        togglePlato = new int[0];
    }

    public MesaDataPrevia(int[] ownerConnectionId,
                    string[] nombrePlatoString,
                    string[] opcionesPlato,
                    string[] cantidadPlatoString,
                    string[] precioPlatoString,
                    int[] togglePlato)
    {
        this.ownerConnectionId = ownerConnectionId;
        this.nombrePlatoString = nombrePlatoString;
        this.opcionesPlato = opcionesPlato;
        this.cantidadPlatoString = cantidadPlatoString;
        this.precioPlatoString = precioPlatoString;
        this.togglePlato = togglePlato;
    }
}