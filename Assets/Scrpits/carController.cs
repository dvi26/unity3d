using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour
{
    private float horizontal, vertical;
    private float anguloGiro, fuerzaFreno;
    private bool estaFrenando;

    [SerializeField] private float fuerzaMotor = 1500f;
    [SerializeField] private float breakForce = 3000f;
    [SerializeField] private float anguloGiroMax = 30f;
    [SerializeField] private float estabilidad = 0.3f;
    [SerializeField] private float traccionAdherencia = 1f;
    
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;
    
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private Rigidbody rb;
    private float puntuacion = 2000;
    private AudioSource audioData;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioData = GetComponent<AudioSource>();
        AjustarFriccionRuedas();
    }

    private void Update()
    {
        ObtenerInput();
        if (puntuacion < 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    private void FixedUpdate()
    {
        ManejarMotor();
        ManejarDireccion();
        ActualizarRuedas();
        AplicarEstabilizacion();
        CorregirInclinacion();
    }

    private void ObtenerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        estaFrenando = Input.GetKey(KeyCode.Space);
    }

    private void ManejarMotor()
    {
        float torque = Mathf.Lerp(0, fuerzaMotor * vertical, Time.deltaTime * 5f);
        rearLeftWheelCollider.motorTorque = torque;
        rearRightWheelCollider.motorTorque = torque;
        
        fuerzaFreno = estaFrenando ? breakForce : 0f;
        AplicarFrenado();
    }

    private void AplicarFrenado()
    {
        frontRightWheelCollider.brakeTorque = fuerzaFreno;
        frontLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearRightWheelCollider.brakeTorque = fuerzaFreno;
    }

    private void ManejarDireccion()
    {
        anguloGiro = Mathf.Lerp(anguloGiro, anguloGiroMax * horizontal, Time.deltaTime * 10f);
        frontLeftWheelCollider.steerAngle = anguloGiro;
        frontRightWheelCollider.steerAngle = anguloGiro;
    }

    private void ActualizarRuedas()
    {
        ActualizarRueda(frontLeftWheelCollider, frontLeftWheelTransform);
        ActualizarRueda(frontRightWheelCollider, frontRightWheelTransform);
        ActualizarRueda(rearRightWheelCollider, rearRightWheelTransform);
        ActualizarRueda(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void ActualizarRueda(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void AplicarEstabilizacion()
    {
        Vector3 velocidadLocal = transform.InverseTransformDirection(rb.linearVelocity);
        rb.AddForce(-transform.right * velocidadLocal.x * estabilidad, ForceMode.Acceleration);
        if (Mathf.Abs(velocidadLocal.x) > 1f)
        {
            rb.AddForce(-transform.right * velocidadLocal.x * traccionAdherencia, ForceMode.Acceleration);
        }
    }

    private void AjustarFriccionRuedas()
    {
        WheelFrictionCurve friccion = rearLeftWheelCollider.sidewaysFriction;
        friccion.stiffness = 2.5f;
        rearLeftWheelCollider.sidewaysFriction = friccion;
        rearRightWheelCollider.sidewaysFriction = friccion;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boosts"))
        {
            StartCoroutine(AumentarVelocidad(other.gameObject, 0f));
            audioData.Play();
        }
        if (other.CompareTag("HyperBoost"))
        {
            StartCoroutine(AumentarVelocidad(other.gameObject, 50000f));
            audioData.Play();
        }
        if (other.CompareTag("Obstacle"))
        {
            StartCoroutine(RestarPuntuacion(other.gameObject));
        }
        if (other.CompareTag("Meta"))
        {
            SceneManager.LoadScene("Fase2");
        }
        SaltoBoost();
    }

    private bool EstaEnSuelo()
    {
        return Physics.Raycast(transform.position, -transform.up, 1.5f);
    }

    private void SaltoBoost()
    {
        if (EstaEnSuelo())
        {
            rb.AddForce(transform.up * 1000f + transform.forward * 500f, ForceMode.Impulse);
        }
    }

    private IEnumerator RestarPuntuacion(GameObject obstaculo)
    {
        obstaculo.SetActive(false);
        yield return new WaitForSeconds(5f);
        puntuacion -= 500;
        obstaculo.SetActive(true);
    }

    private IEnumerator AumentarVelocidad(GameObject boostObjeto, float fuerzaExtra)
    {
        float fuerzaBoost = 9000f + fuerzaExtra;
        float tiempoBoost = 2f;
        float tiempoRespawn = 5f;
        
        rb.AddForce(transform.forward * fuerzaBoost, ForceMode.Impulse);
        
        boostObjeto.SetActive(false);
        yield return new WaitForSeconds(tiempoRespawn);
        boostObjeto.SetActive(true);
    }

    private void CorregirInclinacion()
    {
        Vector3 rotacionCoche = transform.rotation.eulerAngles;
        if (Mathf.Abs(rotacionCoche.x) > anguloGiroMax || Mathf.Abs(rotacionCoche.z) > anguloGiroMax)
        {
            Vector3 correccion = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
            Quaternion rotacionObjetivo = Quaternion.Euler(correccion);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
        }
    }
}
