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


    private void FixedUpdate() {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        if (jumpRequest)
        {
            rb.AddForce(Vector3.up * 10000, ForceMode.Impulse);
            jumpRequest = false;
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
}