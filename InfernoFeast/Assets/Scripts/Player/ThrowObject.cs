using UnityEngine;
using UnityEngine.UI;

public class ThrowObject : MonoBehaviour
{
    [Header("Referencias")]
    public Transform holdParent; // El "Padre" que usas en CogerSoltarObjeto
    public Transform cameraTransform; // Si está vacío, se usará Camera.main
    public Slider powerSlider; // Barra UI que muestra la carga (0..1)

    [Header("Fuerza de lanzamiento")]
    public KeyCode throwKey = KeyCode.Q;
    public float minThrowForce = 4f;
    public float maxThrowForce = 18f;
    public float maxChargeTime = 1.5f; // segundos para llegar a fuerza máxima
    public float additionalUpwards = 0.12f; // añade un poco de inclinación hacia arriba

    [Header("Opciones")]
    public float extraTorque = 2f; // para que rote al lanzarlo
    public bool useCameraForward = true; // usar forward cámara o usar forward del jugador

    // estado interno
    private bool isCharging = false;
    private float chargeTimer = 0f;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
            powerSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        bool holding = (holdParent != null && holdParent.childCount > 0);

        // Solo permitir cargar si hay un objeto en la mano
        if (holding)
        {
            if (Input.GetKeyDown(throwKey))
            {
                StartCharging();
            }

            if (Input.GetKey(throwKey) && isCharging)
            {
                ContinueCharging();
            }

            if (Input.GetKeyUp(throwKey) && isCharging)
            {
                ReleaseThrow();
            }
        }
        else
        {
            // si no tiene objeto, asegurarse de resetear UI
            if (isCharging) StopCharging();
        }
    }

    void StartCharging()
    {
        isCharging = true;
        chargeTimer = 0f;
        if (powerSlider != null) powerSlider.gameObject.SetActive(true);
    }

    void ContinueCharging()
    {
        chargeTimer += Time.deltaTime;
        if (chargeTimer > maxChargeTime) chargeTimer = maxChargeTime;

        float t = (maxChargeTime <= 0f) ? 1f : (chargeTimer / maxChargeTime);
        if (powerSlider != null) powerSlider.value = t;
    }

    void StopCharging()
    {
        isCharging = false;
        chargeTimer = 0f;
        if (powerSlider != null)
        {
            powerSlider.value = 0f;
            powerSlider.gameObject.SetActive(false);
        }
    }

    void ReleaseThrow()
    {
        float t = (maxChargeTime <= 0f) ? 1f : (chargeTimer / maxChargeTime);
        float force = Mathf.Lerp(minThrowForce, maxThrowForce, t);

        // obtener el objeto que se está sosteniendo (se asume 1 hijo)
        GameObject held = holdParent.GetChild(0).gameObject;
        if (held != null)
        {
            // desparentar
            held.transform.SetParent(null, true);

            Rigidbody rb = held.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = held.AddComponent<Rigidbody>();
            }

            // activar física
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // dirección de lanzamiento: forward de la cámara (o del jugador) con un poco hacia arriba
            Vector3 forward = useCameraForward && cameraTransform != null ? cameraTransform.forward : transform.forward;
            forward = forward.normalized + Vector3.up * additionalUpwards;
            forward.Normalize();

            rb.AddForce(forward * force, ForceMode.Impulse);

            // añadir torque para que rote
            rb.AddTorque(Random.onUnitSphere * extraTorque, ForceMode.Impulse);
        }

        StopCharging();
    }
}

