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
    private Image staminaFillImage;

    [Header("Camara")]
    public Transform cameraTransform;
    public Vector3 cameraOffset = new Vector3(0, 5, -7);
    public float cameraFollowSpeed = 5f;
    public bool lookAtPlayer = true;

    [Header("Animator")]
    public Animator animator;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CacheStaminaFillImage();
    }

    private void Start()
    {
        currentSprintTime = Mathf.Max(0.01f, sprintDuration);

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = Mathf.Max(0.01f, sprintDuration);
            staminaSlider.value = currentSprintTime;
        }
    }

    private void Update()
    {
        ReadInput();
        UpdateStaminaUI();
    }

    private void FixedUpdate()
    {
        HandleSprint();
        MovePlayer();
        RotatePlayer();
        FollowCamera();
        UpdateAnimator();
    }

    private void ReadInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void HandleSprint()
    {
        bool hasMovementInput = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift);

        if (hasMovementInput && wantsSprint && currentSprintTime > 0f)
        {
            isSprinting = true;
            currentSprintTime -= Time.fixedDeltaTime;
            currentSprintTime = Mathf.Max(0f, currentSprintTime);
        }
        else
        {
            isSprinting = false;

            if (currentSprintTime < sprintDuration)
            {
                currentSprintTime += sprintRechargeRate * Time.fixedDeltaTime;
                currentSprintTime = Mathf.Min(sprintDuration, currentSprintTime);
            }
        }
    }

    private void MovePlayer()
    {
        if (rb == null)
            return;

        Vector3 forward;
        Vector3 right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
        }
        else
        {
            forward = transform.forward;
            right = transform.right;
        }

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * verticalInput + right * horizontalInput).normalized;

        float finalSpeed = moveSpeed;
        if (isSprinting)
            finalSpeed *= sprintMultiplier;

        rb.velocity = moveDirection * finalSpeed + new Vector3(0f, rb.velocity.y, 0f);
    }

    private void RotatePlayer()
    {
        if (moveDirection == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(-moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    private void FollowCamera()
    {
        if (cameraTransform == null)
            return;

        Vector3 targetPosition = transform.position + cameraOffset;
        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime
        );

        if (lookAtPlayer)
            cameraTransform.LookAt(transform.position);
    }

    private void UpdateAnimator()
    {
        if (animator == null || rb == null)
            return;

        animator.SetFloat("Walk", rb.velocity.magnitude);
        animator.SetBool("isRunning", isSprinting && moveDirection.magnitude > 0.1f);
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider == null)
            return;

        staminaSlider.value = currentSprintTime;

        if (staminaFillImage == null)
            CacheStaminaFillImage();

        if (staminaFillImage != null)
        {
            float safeDuration = Mathf.Max(0.01f, sprintDuration);
            float t = Mathf.Clamp01(currentSprintTime / safeDuration);
            staminaFillImage.color = Color.Lerp(Color.red, Color.green, t);
        }
    }

    private void CacheStaminaFillImage()
    {
        if (staminaSlider != null && staminaSlider.fillRect != null)
            staminaFillImage = staminaSlider.fillRect.GetComponent<Image>();
    }
}