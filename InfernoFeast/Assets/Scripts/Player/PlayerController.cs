using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento del jugador")]
    public float moveSpeed = 5f;
    public Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;

    [Header("Cámara")]
    public Transform cameraTransform; // Cámara que seguirá al jugador
    public Vector3 cameraOffset = new Vector3(0, 5, -7);
    public float cameraFollowSpeed = 5f;
    public bool lookAtPlayer = true;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
            
    }

    void Update()
    {
        // Leer inputs
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        // Movimiento en ejes globales
        Vector3 moveDirection = new Vector3(verticalInput, 0f, horizontalInput).normalized;
        rb.velocity = moveDirection * moveSpeed + new Vector3(0, rb.velocity.y, 0);

        if (cameraTransform != null)
        {
            // Seguir al jugador con suavizado
            Vector3 targetPosition = transform.position + cameraOffset;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPosition,
                cameraFollowSpeed * Time.deltaTime
            );

            if (lookAtPlayer)
                cameraTransform.LookAt(transform.position);
        }
    }

    
}


    
