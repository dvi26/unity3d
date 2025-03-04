using UnityEngine;

public class AsignarTagHijos : MonoBehaviour
{
    void Start()
    {
        // Obtén el tag del objeto padre
        string tagPadre = gameObject.tag;

        // Itera sobre todos los hijos
        foreach (Transform hijo in transform)
        {
            // Asigna el mismo tag al hijo
            hijo.gameObject.tag = tagPadre;
        }
    }
}
