using UnityEngine;
using UnityEngine.UI;

public class CambiarGridLayoutGroup : MonoBehaviour
{
    // Referencia al GridLayoutGroup que queremos modificar
    public GridLayoutGroup gridLayoutGroup;

    // Incremento/decremento deseado del constraintCount
    public int incrementoConstraintCount = 1;

    // Incremento/decremento deseado del tamaño de la celda en Y
    public float incrementoTamañoCeldaY = 10f;

    // Incremento/decremento deseado del tamaño de la celda en X
    public float incrementoTamañoCeldaX = 10f;

    void Start()
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }
    }

    // Método para incrementar el constraintCount del GridLayoutGroup al hacer clic en el botón correspondiente
    public void IncrementarConstraintCount()
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }

        // Incrementar el constraintCount según el incremento especificado
        gridLayoutGroup.constraintCount += incrementoConstraintCount;

        DisminuirTamañoCeldaX(gridLayoutGroup.constraintCount);
    }

    // Método para disminuir el constraintCount del GridLayoutGroup al hacer clic en el botón correspondiente
    public void DisminuirConstraintCount()
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }

        // Verificar que el constraintCount no sea menor que 1 para evitar valores negativos
        if (gridLayoutGroup.constraintCount > 1)
        {
            // Disminuir el constraintCount según el incremento especificado
            gridLayoutGroup.constraintCount -= incrementoConstraintCount;
        }

        IncrementarTamañoCeldaX(gridLayoutGroup.constraintCount);
    }

    // Método para incrementar el tamaño de la celda en Y del GridLayoutGroup al hacer clic en el botón correspondiente
    public void IncrementarTamañoCeldaY()
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }

        // Crear un nuevo vector de tamaño de celda
        Vector2 newSize = gridLayoutGroup.cellSize;

        // Incrementar el tamaño de la celda en Y según el incremento especificado
        newSize.y += incrementoTamañoCeldaY;

        // Asignar el nuevo tamaño de celda al GridLayoutGroup
        gridLayoutGroup.cellSize = newSize;
    }

    // Método para disminuir el tamaño de la celda en Y del GridLayoutGroup al hacer clic en el botón correspondiente
    public void DisminuirTamañoCeldaY()
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }

        // Crear un nuevo vector de tamaño de celda
        Vector2 newSize = gridLayoutGroup.cellSize;

        // Decrementar el tamaño de la celda en Y según el incremento especificado
        newSize.y -= incrementoTamañoCeldaY;

        // Asegurar que el tamaño de la celda no sea menor que 0
        newSize.y = Mathf.Max(newSize.y, 0f);

        // Asignar el nuevo tamaño de celda al GridLayoutGroup
        gridLayoutGroup.cellSize = newSize;
    }

    // Método para incrementar el tamaño de la celda en X del GridLayoutGroup al hacer clic en el botón correspondiente
    public void IncrementarTamañoCeldaX(int n)
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }

        // Crear un nuevo vector de tamaño de celda
        Vector2 newSize = gridLayoutGroup.cellSize;

        // Incrementar el tamaño de la celda en X según el incremento especificado
        //newSize.x += incrementoTamañoCeldaX;
        newSize = new Vector2((1700-10*(n-1))/n, gridLayoutGroup.cellSize.y);

        // Asignar el nuevo tamaño de celda al GridLayoutGroup
        gridLayoutGroup.cellSize = newSize;
    }

    // Método para disminuir el tamaño de la celda en X del GridLayoutGroup al hacer clic en el botón correspondiente
    public void DisminuirTamañoCeldaX(int n)
    {
        // Asegurarse de que gridLayoutGroup no sea nulo
        if (gridLayoutGroup == null)
        {
            Debug.LogError("El GridLayoutGroup no está asignado.");
            return;
        }

        // Crear un nuevo vector de tamaño de celda
        Vector2 newSize = gridLayoutGroup.cellSize;

        // Decrementar el tamaño de la celda en X según el incremento especificado
        //newSize.x -= incrementoTamañoCeldaX;
        newSize = new Vector2((1700-10*(n-1))/n, gridLayoutGroup.cellSize.y);

        // Asegurar que el tamaño de la celda no sea menor que 0
        newSize.x = Mathf.Max(newSize.x, 0f);

        // Asignar el nuevo tamaño de celda al GridLayoutGroup
        gridLayoutGroup.cellSize = newSize;
    }
}