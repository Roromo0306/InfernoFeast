using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento del jugador")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.8f;
    public Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    [Header("Sprint / Stamina")]
    public float sprintDuration = 5f;
    public float sprintRechargeRate = 1f;
    private float currentSprintTime;
    private bool isSprinting;

    [Header("UI - Stamina")]
    public Slider staminaSlider;

    [Header("Camara")]
    public Transform cameraTransform;
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
            cameraTransform = Camera.main.transform;

        currentSprintTime = sprintDuration;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = sprintDuration;
            staminaSlider.value = sprintDuration;
        }
    }

    void Update()
    {
        ReadInput();       
        UpdateStaminaUI();
    }

    void FixedUpdate()
    {
        HandleSprint();
        MovePlayer();
        RotatePlayer();
        FollowCamera();
        UpdateAnimator();
    }

    // 🕹️ Leer inputs
    void ReadInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    // ⚡ Gestionar sprint y stamina
    void HandleSprint()
    {
        // Sprint solo si Shift está pulsado y hay stamina
        if (currentSprintTime > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
            currentSprintTime -= Time.deltaTime;
            if (currentSprintTime < 0) currentSprintTime = 0;
        }
        else
        {
            isSprinting = false;

            // Recarga stamina cuando no está sprintando
            if (currentSprintTime < sprintDuration)
                currentSprintTime += sprintRechargeRate * Time.deltaTime;
        }
    }

    // 🚶 Movimiento del personaje
    void MovePlayer()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        float finalSpeed = moveSpeed; // velocidad normal

        // Solo aplicar multiplicador si puede sprintar
        if (isSprinting)
        {
            finalSpeed *= sprintMultiplier;
        }

        rb.velocity = moveDirection * finalSpeed + new Vector3(0, rb.velocity.y, 0);
    }

    // 🔄 Rotación suave del personaje
    void RotatePlayer()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    // 🎥 Cámara siguiendo al jugador
    void FollowCamera()
    {
        if (cameraTransform == null) return;

        Vector3 targetPosition = transform.position + cameraOffset;
        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );

        if (lookAtPlayer)
            cameraTransform.LookAt(transform.position);
    }

    // 🎬 Actualizar parámetros del Animator
    void UpdateAnimator()
    {
        animator.SetFloat("Walk", rb.velocity.magnitude);
        animator.SetBool("isRunning", isSprinting && moveDirection.magnitude > 0.1f);
    }

    // 💚 Actualizar barra de stamina
    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentSprintTime;

            // Colores opcionales: rojo = poca stamina, verde = llena
            Image fill = staminaSlider.fillRect.GetComponent<Image>();
            float t = currentSprintTime / sprintDuration;
            fill.color = Color.Lerp(Color.red, Color.green, t);
        }
    }
}
