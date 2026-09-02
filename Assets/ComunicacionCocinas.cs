using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class ComunicacionCocinas : NetworkBehaviour
{
    // ── ESTADO AUTORITATIVO EN SERVIDOR ──────────────────────────────
    // Vive solo en el proceso servidor. Evita la condición de carrera de antes,
    // porque usamos HashSet (idempotente) en vez de contar desde el cliente.
    private class GrupoServerState
    {
        public int totCocinas;
        public int totalPlatos;
        public HashSet<int> cocinasListas = new HashSet<int>();
        public HashSet<int> platosCompletados = new HashSet<int>();
        public bool desbloqueado;
    }

    private static Dictionary<string, GrupoServerState> estadosGrupos = new Dictionary<string, GrupoServerState>();
    // Para saber cuál es "el siguiente grupo" a desbloquear cuando este se completa
    private static Dictionary<string, List<int>> secuenciaOrdenesPorComanda = new Dictionary<string, List<int>>();

    private static string ClaveGrupo(string restId, int mesa, int batch, int orden) => $"{restId}_{mesa}_{batch}_{orden}";
    private static string ClaveComanda(string restId, int mesa, int batch) => $"{restId}_{mesa}_{batch}";
    private static int ClaveOrdenamiento(int orden) => orden == 0 ? 999 : orden;

    private string GetRestaurantId()
    {
        return connectionToClient.identity.GetComponent<MyRoomPlayer>().RestaurantID;
    }

    private void BroadcastARestaurante(string restId, System.Action<NetworkConnectionToClient> accion)
    {
        MyRoomManager manager = (MyRoomManager)NetworkManager.singleton;
        if (manager.restaurantConnections.TryGetValue(restId, out List<NetworkConnectionToClient> conns))
        {
            foreach (var c in conns) accion(c);
        }
    }

    // ── REGISTRO DE GRUPO (cada cocina lo llama al instanciar su panel) ──
    [Command]
    public void CmdRegistrarGrupo(int mesa, int batch, int orden, int totCocinas, int totalPlatos, bool esPrimerGrupoDeSecuencia)
    {
        string restId = GetRestaurantId();
        string clave = ClaveGrupo(restId, mesa, batch, orden);

        if (!estadosGrupos.TryGetValue(clave, out GrupoServerState estado))
        {
            estado = new GrupoServerState
            {
                totCocinas = totCocinas,
                totalPlatos = totalPlatos,
                desbloqueado = esPrimerGrupoDeSecuencia
            };
            estadosGrupos[clave] = estado;

            string claveComanda = ClaveComanda(restId, mesa, batch);
            if (!secuenciaOrdenesPorComanda.ContainsKey(claveComanda))
                secuenciaOrdenesPorComanda[claveComanda] = new List<int>();
            if (!secuenciaOrdenesPorComanda[claveComanda].Contains(orden))
            {
                secuenciaOrdenesPorComanda[claveComanda].Add(orden);
                // Reordenamos SIEMPRE numéricamente (0 = "sin orden" va al final, igual que en el cliente)
                secuenciaOrdenesPorComanda[claveComanda].Sort((a, b) => ClaveOrdenamiento(a).CompareTo(ClaveOrdenamiento(b)));
            }
        }

        // Responder solo a quien lo pidió con el estado real actual (por si otra cocina ya avanzó el estado)
        RpcActualizarEstadoGrupo(connectionToClient, mesa, batch, orden, estado.cocinasListas.Count, estado.desbloqueado);
    }

    // ── TOGGLE "ESTA COCINA ESTÁ LISTA" ──────────────────────────────
    [Command]
    public void CmdGrupoListo(int mesa, int batch, int orden, int numeroCocina, int totCocinas, int totalPlatos)
    {
        string restId = GetRestaurantId();
        string clave = ClaveGrupo(restId, mesa, batch, orden);

        if (!estadosGrupos.TryGetValue(clave, out GrupoServerState estado))
        {
            estado = new GrupoServerState { totCocinas = totCocinas, totalPlatos = totalPlatos, desbloqueado = true };
            estadosGrupos[clave] = estado;
        }

        if (!estado.desbloqueado)
        {
            Debug.LogWarning($"[CmdGrupoListo] Intento de marcar listo un grupo bloqueado: {clave}");
            return; // seguridad extra por si el cliente estaba desincronizado
        }

        estado.cocinasListas.Add(numeroCocina); // HashSet: idempotente, sin condición de carrera

        Debug.Log($"[CmdGrupoListo] Grupo {clave} -> cocinasListas={estado.cocinasListas.Count}/{estado.totCocinas}");

        BroadcastARestaurante(restId, c =>
            RpcActualizarEstadoGrupo(c, mesa, batch, orden, estado.cocinasListas.Count, estado.desbloqueado));
    }

    // ── UN PLATO SE MARCA COMO COMPLETADO ────────────────────────────
    [Command]
    public void CmdPlatoCompletadoGrupo(int mesa, int batch, int orden, int totalPlatosGrupo)
    {
        string restId = GetRestaurantId();
        string clave = ClaveGrupo(restId, mesa, batch, orden);

        if (!estadosGrupos.TryGetValue(clave, out GrupoServerState estado))
        {
            estado = new GrupoServerState { totalPlatos = totalPlatosGrupo, desbloqueado = false };
            estadosGrupos[clave] = estado;
        }

        // Nota: aquí solo contamos "cuántos platos se han marcado en total para este grupo",
        // sin distinguir índice global de plato (usamos un contador simple porque
        // cada cocina solo puede marcar SUS propios platos una vez). Si prefieres
        // más robustez, cambia esto por un HashSet de un índice global de plato.
        estado.platosCompletados.Add(estado.platosCompletados.Count); // incremento simple

        if (estado.platosCompletados.Count >= estado.totalPlatos)
        {
            DesbloquearSiguienteGrupo(restId, mesa, batch, orden);
        }
    }

    private void DesbloquearSiguienteGrupo(string restId, int mesa, int batch, int ordenCompletado)
    {
        string claveComanda = ClaveComanda(restId, mesa, batch);
        if (!secuenciaOrdenesPorComanda.TryGetValue(claveComanda, out List<int> secuencia)) return;

        int posicion = secuencia.IndexOf(ordenCompletado);
        if (posicion < 0 || posicion + 1 >= secuencia.Count) return; // no hay siguiente grupo

        int siguienteOrden = secuencia[posicion + 1];
        string claveSiguiente = ClaveGrupo(restId, mesa, batch, siguienteOrden);

        if (!estadosGrupos.TryGetValue(claveSiguiente, out GrupoServerState estadoSiguiente))
        {
            // el siguiente grupo aún no se ha registrado (esa cocina no lo ha instanciado todavía);
            // creamos el estado igualmente para que cuando se registre, ya nazca desbloqueado.
            estadoSiguiente = new GrupoServerState();
            estadosGrupos[claveSiguiente] = estadoSiguiente;
        }

        estadoSiguiente.desbloqueado = true;

        BroadcastARestaurante(restId, c =>
            RpcActualizarEstadoGrupo(c, mesa, batch, siguienteOrden, estadoSiguiente.cocinasListas.Count, true));
    }

    // ── RPC: EL CLIENTE ACTUALIZA SU PANEL ───────────────────────────
    [TargetRpc]
    void RpcActualizarEstadoGrupo(NetworkConnectionToClient conn, int mesa, int batch, int orden, int cocinasReady, bool desbloqueado)
    {
        if (SceneManager.GetActiveScene().name != "CocinaScene") return;

        GameObject contentComandas = GameObject.FindGameObjectWithTag("contentCocina");
        if (contentComandas == null) return;

        string claveBuscada = $"Grupo_Mesa{mesa}_Batch{batch}_Orden{orden}";

        // Buscamos en todas las comandas activas (puede estar en cualquier hijo, ya que
        // el panel de grupo está parenteado dentro de "Content" de la comanda correspondiente)
        GrupoCocinaUI[] gruposEnEscena = contentComandas.GetComponentsInChildren<GrupoCocinaUI>(true);
        foreach (GrupoCocinaUI grupo in gruposEnEscena)
        {
            if (grupo.ClaveGrupo != claveBuscada) continue;

            Debug.Log($"[RpcActualizarEstadoGrupo] Actualizando {claveBuscada} con cocinasReady={cocinasReady}, desbloqueado={desbloqueado}");

            grupo.desbloqueado = desbloqueado;
            grupo.RefrescarVisual(cocinasReady);
            break;
        }
    }
}