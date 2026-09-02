using System;

public static class FichajeEvents
{
    public static event Action OnFichajeRegistrado;
    public static void RaiseFichajeRegistrado() => OnFichajeRegistrado?.Invoke();

    public static event Action OnFichajeCodigoInvalido;
    public static void RaiseFichajeCodigoInvalido() => OnFichajeCodigoInvalido?.Invoke();
}