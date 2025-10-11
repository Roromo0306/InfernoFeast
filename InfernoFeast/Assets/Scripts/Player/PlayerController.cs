using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento del jugador")]
    public float moveSpeed = 5f;
    public Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;

    [Header("Camara")]
    public Transform cameraTransform; // Camara que seguirá al jugador
    public Vector3 cameraOffset = new Vector3(0, 5, -7);
    public float cameraFollowSpeed = 5f;
    public bool lookAtPlayer = true;

    [Header("Animator")]
    public Animator animator;

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
        // Movimiento relativo a la cámara
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Mantener el movimiento en el plano XZ
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Combinar inputs con los ejes de la cámara
        Vector3 moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        // Aplicar velocidad
        rb.velocity = moveDirection * moveSpeed + new Vector3(0, rb.velocity.y, 0);

        animator.SetFloat("Walk", rb.velocity.magnitude);

        // Seguir al jugador con suavizado
        if (cameraTransform != null)
        {
            Vector3 targetPosition = transform.position + cameraOffset;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPosition,
                cameraFollowSpeed * Time.deltaTime
            );

            if (lookAtPlayer)
                cameraTransform.LookAt(transform.position);
        }

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }
}


