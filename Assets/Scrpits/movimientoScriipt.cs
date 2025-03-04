using UnityEngine;

public class MovimientoScript : MonoBehaviour
{
    // Variables de movimiento
    private float horizontal;  // Input para movimiento horizontal (izquierda/derecha)
    private float vertical;    // Input para movimiento vertical (adelante/atrás)
    public float velocidad;    // Velocidad de movimiento del jugador
    private Rigidbody rb;      // Referencia al Rigidbody del jugador para aplicar fuerzas
    private bool isGrounded = false;  // Verifica si el jugador está tocando el suelo
    private Collision toque;   // Referencia para el objeto con el que el jugador colisiona

    // Start se ejecuta una vez al inicio, antes del primer Update
    void Start()
    {
        // Obtener el componente Rigidbody que está en el mismo GameObject
        rb = GetComponent<Rigidbody>();
    }

    // Update se ejecuta una vez por frame
    void Update()
    {
        // Obtener los inputs de movimiento (horizontal y vertical) del jugador
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Crear un vector de movimiento basado en los inputs
        Vector3 movimiento = new Vector3(horizontal, 0f, vertical);

        // Aplicar fuerza al Rigidbody para mover el jugador
        rb.AddForce(movimiento * velocidad);    

        // Verificar si el jugador está presionando la tecla "Espacio" y si está en el suelo
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Aplicar una fuerza hacia arriba para hacer el salto
            rb.AddForce(Vector3.up * 25, ForceMode.Impulse);
        }
    }

    // Detectar cuando el jugador colisiona con un objeto
    void OnCollisionEnter(Collision toque)
    {
        // Comprobar si la colisión es con el suelo (objeto con el tag "Ground")
        if (toque.gameObject.CompareTag("Ground"))
        {
            // Si está tocando el suelo, permitir saltar
            isGrounded = true;
        }
    }

    // Detectar cuando el jugador deja de estar en contacto con un objeto
    void OnCollisionExit(Collision toque)
    {
        // Si el jugador deja el suelo, no podrá saltar
        isGrounded = false;
    }
}
