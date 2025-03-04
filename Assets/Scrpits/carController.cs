using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    // Variables de movimiento y frenos
    private float horizontal, vertical;
    private float anguloGiro, fuerzaFreno;
    private bool estaFrenando;
    private Vector3 ultimaPosicionEnGround;


    // Variables de configuración, modificables en el editor
    [SerializeField] private float fuerzaMotor = 1500f;
    [SerializeField] private float breakForce = 3000f;
    [SerializeField] private float anguloGiroMax = 30f;
    [SerializeField] private float estabilidad = 0.3f;
    [SerializeField] private float traccionAdherencia = 1f;
    
    // Referencias a los WheelColliders de las ruedas
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
    
    // Referencias a las Transformaciones de las ruedas (para mostrar la rotación)
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;
    private PuntuacionController puntuacionController; // Referencia al controlador de puntuación

    // Variables internas
    private Rigidbody rb;
    public float puntuacion = 2000; // Puntuación del jugador (publica para que pueda ser usada por otros scripts)
    private AudioSource audioData;
    private Vector3 ultimaPosicionCheckpoint;  // Para almacenar la última posición del checkpoint

private bool isResettingPosition = false;  // Flag para saber si estamos reseteando la posición
private float timeHeldR = 0f;  // Tiempo que se ha mantenido presionada la tecla "R"
private float resetTimeThreshold = 3f;  // Tiempo necesario para hacer el reset (en segundos)
private Vector3 posicionInicial;  // Para almacenar la posición inicial del coche


    private void Start()
    {
        // Inicializa el Rigidbody y el AudioSource
        rb = GetComponent<Rigidbody>();
        audioData = GetComponent<AudioSource>();
        posicionInicial = transform.position;

        // Ajusta la fricción de las ruedas al inicio
        AjustarFriccionRuedas();
        puntuacionController = FindFirstObjectByType<PuntuacionController>();
    }

    private void Update()
    {
       

        // Si el boost está activo, decrementamos el tiempo restante
    if (boostTiempoRestante > 0)
    {
        boostTiempoRestante -= Time.deltaTime;  // Decrementamos el tiempo de boost
    }
    else
    {
        // Cuando se agote el tiempo del boost, aseguramos que la velocidad limite sea la normal
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, velocidadMaximaNormal);  
    }

    // Si la puntuación es negativa, termina el juego
    if (puntuacion <= 0)
    {
        SceneManager.LoadScene("GameOver");
    }
    if (Input.GetKey(KeyCode.R))
{
    // Si "R" está presionado, incrementamos el tiempo
    timeHeldR += Time.deltaTime;

    if (timeHeldR >= resetTimeThreshold && !isResettingPosition)
    {
        isResettingPosition = true;  // Marca que se está reseteando la posición
        ResetearPosicion();  // Llama a la función para resetear la posición
    }
}
else
{
    // Si "R" no está presionado, reinicia el contador de tiempo
    timeHeldR = 0f;
    isResettingPosition = false;  // Restablece el flag
}
}

    private void FixedUpdate()
    {
         // Lee las entradas del jugador (teclas)
        ObtenerInput();
         // Limitar la velocidad máxima en función del estado del boost
    float velocidadMaxima = (boostTiempoRestante > 0) ? velocidadMaximaBoost : velocidadMaximaNormal;  
    rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, velocidadMaxima);  // Limita la velocidad del coche

        // Actualiza las acciones del vehículo en cada frame de física
        ManejarMotor();
        ManejarDireccion();
        ActualizarRuedas();
        AplicarEstabilizacion();
        CorregirInclinacion();
    }

    private void ObtenerInput()
{
        horizontal = Input.GetAxis("Horizontal");  // Dirección (izquierda/derecha)
        vertical = Input.GetAxis("Vertical");  // Aceleración (adelante/atrás)
        estaFrenando = Input.GetKey(KeyCode.Space);  // Si está frenando (espacio)
        /*
    // Solo permite mover el coche si está en el suelo
    if (EstaEnSuelo())
    {
        // Si está en el suelo, se pueden obtener las entradas del jugador
        horizontal = Input.GetAxis("Horizontal");  // Dirección (izquierda/derecha)
        vertical = Input.GetAxis("Vertical");  // Aceleración (adelante/atrás)
        estaFrenando = Input.GetKey(KeyCode.Space);  // Si está frenando (espacio)
    }
    else
    {
        // Si está en el aire, se desactivan las entradas de movimiento, pero no las freno
        // Solo desactiva movimiento, no freno
        horizontal = 0f;
        vertical = 0f;
    }*/
}
private void ResetearPosicion()
{
        transform.position = posicionInicial;
    // Rotar el coche 180 grados en el eje Y (mirar en dirección opuesta)
    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
}


    // Controla el motor, calculando el torque para mover las ruedas traseras
    private void ManejarMotor()
    {
        float torque = Mathf.Lerp(0, fuerzaMotor * vertical, Time.deltaTime * 5f);
        rearLeftWheelCollider.motorTorque = torque;
        rearRightWheelCollider.motorTorque = torque;
        
        // Si está frenando, aplica la fuerza de freno
        fuerzaFreno = estaFrenando ? breakForce : 0f;
        AplicarFrenado();
    }

    // Aplica la fuerza de freno a todas las ruedas
    private void AplicarFrenado()
    {
        frontRightWheelCollider.brakeTorque = fuerzaFreno;
        frontLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearRightWheelCollider.brakeTorque = fuerzaFreno;
    }

    // Controla la dirección del vehículo
    private void ManejarDireccion()
    {
        anguloGiro = Mathf.Lerp(anguloGiro, anguloGiroMax * horizontal, Time.deltaTime * 10f);
        frontLeftWheelCollider.steerAngle = anguloGiro;
        frontRightWheelCollider.steerAngle = anguloGiro;
    }

    // Actualiza las posiciones y rotaciones de las ruedas en la escena
    private void ActualizarRuedas()
    {
        ActualizarRueda(frontLeftWheelCollider, frontLeftWheelTransform);
        ActualizarRueda(frontRightWheelCollider, frontRightWheelTransform);
        ActualizarRueda(rearRightWheelCollider, rearRightWheelTransform);
        ActualizarRueda(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    // Actualiza la posición y rotación de cada rueda
    private void ActualizarRueda(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    // Aplica estabilización al coche, corrigiendo el movimiento lateral
    private void AplicarEstabilizacion()
    {
        Vector3 velocidadLocal = transform.InverseTransformDirection(rb.linearVelocity);
        rb.AddForce(-transform.right * velocidadLocal.x * estabilidad, ForceMode.Acceleration);

        // Aplica tracción adicional si el vehículo se mueve demasiado rápido
        if (Mathf.Abs(velocidadLocal.x) > 1f)
        {
            rb.AddForce(-transform.right * velocidadLocal.x * traccionAdherencia, ForceMode.Acceleration);
        }
    }

    // Ajusta la fricción de las ruedas traseras para mejor manejo
    private void AjustarFriccionRuedas()
    {
        WheelFrictionCurve friccion = rearLeftWheelCollider.sidewaysFriction;
        friccion.stiffness = 2.5f;
        rearLeftWheelCollider.sidewaysFriction = friccion;
        rearRightWheelCollider.sidewaysFriction = friccion;
    }

    // Detecta las colisiones con objetos de la escena
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boosts"))
        {
            // Activa un boost temporal al tocar un objeto de boost
            StartCoroutine(AumentarVelocidad(other.gameObject, 0f));
            audioData.Play();
        }
        else if (other.CompareTag("HyperBoost"))
        {
            // Activa un boost de velocidad extra
            StartCoroutine(AumentarVelocidad(other.gameObject, 25000f));
            audioData.Play();
        }
        else if (other.CompareTag("Obstacle"))
    {
        // Llama a la función RestarPuntuacion cuando colisiona con un obstáculo
        RestarPuntuacion(other.gameObject);  // Llama al método de restar puntos
    }
        else if (other.CompareTag("Meta"))
        {
            // Carga la siguiente fase al pasar la meta
            SceneManager.LoadScene("MainScene");
        }
        // Si colisiona con el tag "Terreno"
if (other.CompareTag("Terreno"))
{
    // Vuelve a la última posición registrada del tag "Ground"
    if (ultimaPosicionEnGround != Vector3.zero)
    {
        transform.position = ultimaPosicionEnGround;
    }
}
// Verificar si el coche toca un checkpoint
    if (other.CompareTag("CheckPoints"))
    {
        // Actualiza la posición del último checkpoint
        posicionInicial = transform.position;
    }
// Si colisiona con el tag "Ground"
else if (other.CompareTag("Ground"))
{
    // Guarda la posición actual del coche cuando toque un objeto con el tag "Ground"
    ultimaPosicionEnGround = transform.position;
}


        // Ejecuta un salto adicional al tocar cualquier boost
        SaltoBoost();
    }

    // Verifica si el coche está tocando el suelo
    private bool EstaEnSuelo()
    {
        return Physics.Raycast(transform.position, -transform.up, 1.5f);
    }

    // Aplica un salto con un boost si el coche está en el suelo
    private void SaltoBoost()
    {
        if (EstaEnSuelo())
        {
            rb.AddForce(transform.up * 1000f + transform.forward * 500f, ForceMode.Impulse);
        }
    }
private bool canReduceScore = true;  // Flag para controlar si se puede restar puntos
private float cooldownTime = 1f;     // Tiempo de cooldown en segundos (puedes aumentar este valor si es necesario)

private void RestarPuntuacion(GameObject obstaculo)
{
    if (canReduceScore && obstaculo.activeInHierarchy)  // Verifica si el cooldown ha pasado
    {
        canReduceScore = false;  // Activa el cooldown (ya no se pueden restar puntos inmediatamente)

        obstaculo.SetActive(false);  // Desactiva el obstáculo inmediatamente (desaparece visualmente)

        puntuacion -= 500;  // Resta 500 puntos

        // Actualiza la puntuación en la UI
        if (puntuacionController != null)
        {
            puntuacionController.ActualizarPuntuacion(puntuacion);  // Llama al método de PuntuacionController
        }

        // Verifica si la puntuación es negativa
        if (puntuacion < 0)
        {
            SceneManager.LoadScene("GameOver");  // Fin del juego si la puntuación es negativa
        }

        // Inicia el cooldown para volver a restar puntos después de 1 segundo
        StartCoroutine(CooldownRestarPuntuacion(obstaculo));  // Pasa el objeto para reactivarlo después
    }
}

private IEnumerator CooldownRestarPuntuacion(GameObject obstaculo)
{
    // Espera durante el tiempo de cooldown (1 segundo)
    yield return new WaitForSeconds(cooldownTime);  // Ajusta cooldownTime si es necesario (en segundos)

    // Asegúrate de que el objeto no se reactivará demasiado rápido:
    yield return new WaitForSeconds(1.6f);  // Añadimos un pequeño retraso de 1.6 segundos más para que el objeto respawnee

    // Ahora se asegura que solo se reactive después del cooldown
    obstaculo.SetActive(true);  // Reactiva el obstáculo para que vuelva a ser visible

    // Permite restar puntos nuevamente después del cooldown
    canReduceScore = true;
}
private float boostTiempoRestante = 0f;  // Tiempo restante para el boost
private float velocidadMaximaNormal = 150f;  // Velocidad máxima normal (sin boost)
private float velocidadMaximaBoost = 300f;  // Velocidad máxima durante el boost
    private IEnumerator AumentarVelocidad(GameObject boostObjeto, float fuerzaExtra)
{
    // Si el boost ya está activo, no reiniciamos el tiempo de boost
    if (boostTiempoRestante <= 0)
    {
        boostTiempoRestante = 5f;  // El boost durará 5 segundos
        rb.AddForce(transform.forward * (9000f + fuerzaExtra), ForceMode.Impulse);  // Aumenta la velocidad

        boostObjeto.SetActive(false);  // Desactiva el boost
        yield return new WaitForSeconds(5f);  // Tiempo de boost

        // Reactiva el boost después de un tiempo
        boostObjeto.SetActive(true);
    }
}

    // Corrige la inclinación del coche para mantenerlo estable
    private void CorregirInclinacion()
    {
        Vector3 rotacionCoche = transform.rotation.eulerAngles;
        if (Mathf.Abs(rotacionCoche.x) > anguloGiroMax || Mathf.Abs(rotacionCoche.z) > anguloGiroMax)
        {
            // Corrige el coche en el eje X y Z para que no se voltee
            Vector3 correccion = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
            Quaternion rotacionObjetivo = Quaternion.Euler(correccion);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
        }
    }
}
