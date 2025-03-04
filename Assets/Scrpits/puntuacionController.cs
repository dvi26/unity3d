using UnityEngine;
using TMPro;
using System.Collections;

public class PuntuacionController : MonoBehaviour
{
    // Referencias públicas a componentes de la interfaz y al controlador del coche
    [SerializeField] private TextMeshProUGUI scoreText; // Referencia al texto que muestra la puntuación
    [SerializeField] private CarController carController; // Referencia al CarController para acceder a la puntuación del jugador

    // Parametros para la animación de escala
    [SerializeField] private float bounceScaleFactor = 1.2f; // Factor de escala al hacer "rebote"
    [SerializeField] private float bounceDuration = 0.5f; // Duración del rebote
    private Vector3 originalScale;

    // Start se ejecuta una vez al inicio, antes del primer Update
    void Start()
    {
        // Guardar la escala original del texto
        originalScale = scoreText.transform.localScale;

        // Establecer la puntuación inicial
        if (scoreText != null && carController != null)
        {
            scoreText.text = "Puntuación: " + carController.puntuacion.ToString();
        }
    }

    // Update se ejecuta una vez por frame
    void Update()
    {
        // Verificamos si las referencias están asignadas
    if (scoreText != null && carController != null)
    {
        // Actualizar la puntuación en la interfaz
        string nuevaPuntuacion = "Puntuación: " + carController.puntuacion.ToString();

        // Verificar si la puntuación ha cambiado
        if (scoreText.text != nuevaPuntuacion)
        {
            // Animar la puntuación al cambiar
            StartCoroutine(AnimarPuntuacion());
        }

        // Actualizar el texto
        scoreText.text = nuevaPuntuacion;
    }
    }

    // Corutina para animar el rebote de la puntuación
    private IEnumerator AnimarPuntuacion()
    {
        // Hacer que el texto rebote
        float elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            // Aplicar escala de rebote
            float scale = Mathf.Lerp(1f, bounceScaleFactor, Mathf.PingPong(elapsedTime * 2f, 1f));
            scoreText.transform.localScale = originalScale * scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegurarse de que la escala vuelva a la normalidad después del rebote
        scoreText.transform.localScale = originalScale;
    }

    public void ActualizarPuntuacion(float nuevaPuntuacion)
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntuación: " + nuevaPuntuacion.ToString();
        }
    }
}
