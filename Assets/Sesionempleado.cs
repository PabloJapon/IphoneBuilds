using System.Collections.Generic;

/// <summary>
/// Sesión del empleado en memoria. No se guarda en disco (ni PlayerPrefs
/// ni archivos), así que se pierde al cerrar la app y obliga a iniciar
/// sesión cada vez que se abre.
/// </summary>

public static class SesionEmpleado
{
    public static string RestaurantId;
    public static int IdEmpleado;
    public static string Codigo;
    public static List<string> Permisos = new List<string>();

    public static bool HaySesion => IdEmpleado != 0 && !string.IsNullOrEmpty(RestaurantId);

    public static void CerrarSesion()
    {
        RestaurantId = null;
        IdEmpleado = 0;
    }
}