using UnityEngine;

public class movimientoScriipt : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    public float velocidad;
    private Rigidbody rb;
    private bool isGrounded= false;
private Collision toque;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
horizontal = Input.GetAxis("Horizontal");
vertical = Input.GetAxis("Vertical");

Vector3 movimiento = 
new Vector3(horizontal, 0f, vertical);

rb.AddForce(movimiento * velocidad);    

// Jump when space is pressed
if (Input.GetKeyDown("space") && isGrounded)
{
    rb.AddForce(Vector3.up * 25, ForceMode.Impulse);
}


 

    }
    void OnCollisionEnter(Collision toque)
 {
     if (toque.gameObject.CompareTag("Ground"))
     {
         isGrounded = true;
     }
 }
 void OnCollisionExit(Collision toque)
 {
     isGrounded = false;
 }
}
