using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform car;  // El coche que la cámara debe seguir
    public Vector3 offset = new Vector3(0, 5, 8); // Desplazamiento desde el coche, en este caso (0, 5, 8)
    public float rotationSpeed = 5f;  // Velocidad de rotación de la cámara
    public float followSpeed = 10f; // Velocidad de seguimiento de la cámara
    public float pitch = 5f; // Inclinación de la cámara

    private float currentRotation = 0f;

    private void Start()
    {
        if (car == null)
        {
            Debug.LogError("El coche no está asignado en la cámara.");
        }
    }

    private void LateUpdate()
    {
        if (car != null)
        {
            // Calculamos la posición deseada de la cámara basada en el offset
            Vector3 targetPosition = car.position + offset;

            // Mover la cámara suavemente hacia la posición deseada
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

            // Obtener la rotación del coche alrededor del eje Y (mantener el "norte")
            currentRotation = car.eulerAngles.y;

            // Calculamos la rotación de la cámara, manteniendo el "norte" (sin rotar completamente)
            Quaternion targetRotation = Quaternion.Euler(pitch, currentRotation, 0f);

            // Suavizar la rotación de la cámara hacia la rotación deseada
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Asegurarse de que el coche siempre esté centrado en la cámara
            Vector3 directionToCar = car.position - transform.position;
            transform.position = car.position - directionToCar.normalized * offset.magnitude;
        }
    }
}
