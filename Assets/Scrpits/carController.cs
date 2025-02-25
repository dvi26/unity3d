using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class carController : MonoBehaviour
{
    private float horizontal, vertical;
    private float anguloGiro, fuerzaFreno;
    private bool estaFrenando;

    // Configuración de manejo
    [SerializeField] private float fuerzaMotor = 1500f;
    [SerializeField] private float breakForce = 3000f;
    [SerializeField] private float anguloGiroMax = 30f;
    [SerializeField] private float estabilidad = 0.3f;
    [SerializeField] private float traccionAdherencia = 1f;
    
    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Transforms de las ruedas
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private Rigidbody rb;
    private float puntuacion = 2000;
    private AudioSource audioData;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioData = GetComponent<AudioSource>();
        AdjustWheelFriction();
    }

    private void Update()
    {
        GetInput();
        if (puntuacion < 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        ApplyStabilization();
        CheckAndCorrectTilt();
    }

    private void GetInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        estaFrenando = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        float torque = Mathf.Lerp(0, fuerzaMotor * vertical, Time.deltaTime * 5f);
        rearLeftWheelCollider.motorTorque = torque;
        rearRightWheelCollider.motorTorque = torque;

        fuerzaFreno = estaFrenando ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = fuerzaFreno;
        frontLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearRightWheelCollider.brakeTorque = fuerzaFreno;
    }

    private void HandleSteering()
    {
        anguloGiro = Mathf.Lerp(anguloGiro, anguloGiroMax * horizontal, Time.deltaTime * 10f);
        frontLeftWheelCollider.steerAngle = anguloGiro;
        frontRightWheelCollider.steerAngle = anguloGiro;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void ApplyStabilization()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

        // Aplicar fuerza de estabilidad
        rb.AddForce(-transform.right * localVelocity.x * estabilidad, ForceMode.Acceleration);

        // Ajustar adherencia en curvas
        if (Mathf.Abs(localVelocity.x) > 1f)
        {
            rb.AddForce(-transform.right * localVelocity.x * traccionAdherencia, ForceMode.Acceleration);
        }
    }

    private void AdjustWheelFriction()
    {
        WheelFrictionCurve friction = rearLeftWheelCollider.sidewaysFriction;
        friction.stiffness = 2.5f;
        rearLeftWheelCollider.sidewaysFriction = friction;
        rearRightWheelCollider.sidewaysFriction = friction;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boosts"))
        {
            StartCoroutine(BoostSpeed(other.gameObject));
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
    }

    private IEnumerator RestarPuntuacion(GameObject obstacle)
    {
        obstacle.SetActive(false);
        yield return new WaitForSeconds(5f);
        puntuacion -= 500;
        obstacle.SetActive(true);
    }

    private IEnumerator BoostSpeed(GameObject boostObject)
    {
        float boostForce = 9000f;
        float boostDuration = 2f;
        float respawnTime = 5f;

        rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);

        boostObject.SetActive(false);
        yield return new WaitForSeconds(respawnTime);
        boostObject.SetActive(true);
    }
    private void CheckAndCorrectTilt()
    {
        // Obtener la rotación del coche en los ejes X y Z
        Vector3 carRotation = transform.rotation.eulerAngles;

        // Comprobar si el coche está inclinado en el eje X o Z
        if (Mathf.Abs(carRotation.x) > anguloGiroMax || Mathf.Abs(carRotation.z) > anguloGiroMax)
        {
            // Si el coche está inclinado más de lo permitido, corregirlo
            Vector3 correction = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
            Quaternion targetRotation = Quaternion.Euler(correction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Aplicar corrección suave
        }
    }
}
