using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Referencia al jugador y su Rigidbody
    public Transform player;  // Transform del jugador, usado para seguirlo
    private Rigidbody playerRB;  // Rigidbody del jugador, usado para obtener su velocidad

    // Offset y velocidad de movimiento de la cámara
    public Vector3 Offset;  // Desplazamiento de la cámara con respecto al jugador
    public float speed;  // Velocidad de movimiento de la cámara

    // Start se ejecuta al inicio, cuando la escena es cargada
    void Start()
    {
        // Obtiene el Rigidbody del jugador para trabajar con la velocidad
        playerRB = player.GetComponent<Rigidbody>();
    }

    // LateUpdate se ejecuta después de que todos los Updates se han ejecutado, asegurando que la cámara se actualice después del movimiento del jugador
    void LateUpdate()
    {
        // Calcula la dirección hacia adelante del jugador sumando su velocidad y la dirección hacia donde está mirando
        Vector3 playerForward = (playerRB.linearVelocity + player.transform.forward).normalized;

        // Calcula la nueva posición de la cámara usando la interpolación (Lerp) entre la posición actual de la cámara y la nueva posición deseada
        // La posición deseada es el jugador más el offset, con un ajuste adicional hacia adelante (hacia donde está mirando el jugador)
        transform.position = Vector3.Lerp(
            transform.position,  // Posición actual de la cámara
            player.position + player.transform.TransformVector(Offset) + playerForward * -5f,  // Nueva posición calculada
            speed * Time.deltaTime  // Velocidad de movimiento de la cámara
        );

        // Hace que la cámara siempre mire al jugador
        transform.LookAt(player);
    }
}
