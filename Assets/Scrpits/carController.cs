using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carController : MonoBehaviour
{
    private float horizontal, vertical;
    private float anguloGiro, fuerzaFreno;
    private bool estaFrenando;

    // Settings
    [SerializeField] private float fuerzaMotor, breakForce, anguloGiroMax;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;
    private Rigidbody rb;
    private bool jumpRequest = false;

    [SerializeField] private float maxTiltAngle = 45f; // Ángulo máximo de inclinación permitido

    private void FixedUpdate() {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        // Verificar la inclinación y aplicar corrección si es necesario
        CheckAndCorrectTilt();

        
        if (jumpRequest)
        {
            rb.AddForce(Vector3.up * 10000, ForceMode.Impulse);
            jumpRequest = false;
        }
        if (vertical>0.75){
            vertical=0;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetInput();

        // Detección de salto en Update()
        if (Input.GetKeyDown(KeyCode.Q) && IsCarGrounded())
        {
            jumpRequest = true;
        }
    }

    private void GetInput() {
        // Steering Input
        horizontal = Input.GetAxis("Horizontal");

        // Acceleration Input
        vertical = Input.GetAxis("Vertical");

        // Breaking Input
        estaFrenando = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor() {
        frontLeftWheelCollider.motorTorque = vertical * fuerzaMotor;
        frontRightWheelCollider.motorTorque = vertical * fuerzaMotor;
        fuerzaFreno = estaFrenando ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking() {
        frontRightWheelCollider.brakeTorque = fuerzaFreno;
        frontLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearLeftWheelCollider.brakeTorque = fuerzaFreno;
        rearRightWheelCollider.brakeTorque = fuerzaFreno;
    }

    private void HandleSteering() {
        anguloGiro = anguloGiroMax * horizontal;
        frontLeftWheelCollider.steerAngle = anguloGiro;
        frontRightWheelCollider.steerAngle = anguloGiro;
    }

    private void UpdateWheels() {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform) {
        Vector3 pos;
        Quaternion rot; 
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private bool IsCarGrounded()
    {
        return frontLeftWheelCollider.isGrounded ||
        frontRightWheelCollider.isGrounded ||
        rearLeftWheelCollider.isGrounded ||
        rearRightWheelCollider.isGrounded;
    }

    // Nueva función para comprobar la inclinación y corregirla si es necesario
    private void CheckAndCorrectTilt()
    {
        // Obtener la rotación del coche en los ejes X y Z
        Vector3 carRotation = transform.rotation.eulerAngles;

        // Comprobar si el coche está inclinado en el eje X o Z
        if (Mathf.Abs(carRotation.x) > maxTiltAngle || Mathf.Abs(carRotation.z) > maxTiltAngle)
        {
            // Si el coche está inclinado más de lo permitido, corregirlo
            Vector3 correction = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
            Quaternion targetRotation = Quaternion.Euler(correction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Aplicar corrección suave
        }
    }
}
