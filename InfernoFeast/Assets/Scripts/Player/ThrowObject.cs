using UnityEngine;
using UnityEngine.UI;

public class ThrowObject : MonoBehaviour
{
    public enum ThrowMode { ImpulseOld, ParabolicAngle, ParabolicToDistance }

    [Header("Referencias")]
    public Transform holdParent;      // empty dentro del player que contiene el objeto
    public Transform cameraTransform; // Camera.main por defecto si no asignada
    public Slider powerSlider;

    [Header("Fuerza de lanzamiento")]
    public KeyCode throwKey = KeyCode.Q;
    public float minThrowForce = 4f;
    public float maxThrowForce = 18f;
    public float maxChargeTime = 1.5f;

    [Header("Modo y apuntado")]
    public ThrowMode throwMode = ThrowMode.ParabolicAngle;
    public bool useCameraForward = true;
    [Tooltip("Ángulo en grados para ParabolicAngle / ParabolicToDistance")]
    public float fixedAngleDegrees = 20f;
    public float targetDistance = 8f;
    public float raycastMaxDistance = 100f;

    [Header("Física / Debug")]
    public float extraTorque = 2f;
    public bool drawDebug = true; // activa Debug.DrawRay y Debug.Log

    [Header("Ajustes adicionales")]
    [Tooltip("Invierte el sentido final del lanzamiento (multiplica la velocidad/fuerza por -1).")]
    public bool invertThrowDirection = false;

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

        if (holding)
        {
            if (Input.GetKeyDown(throwKey)) StartCharging();
            if (Input.GetKey(throwKey) && isCharging) ContinueCharging();
            if (Input.GetKeyUp(throwKey) && isCharging) ReleaseThrow();
        }
        else
        {
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
        float speed = Mathf.Lerp(minThrowForce, maxThrowForce, t);

        if (holdParent == null || holdParent.childCount == 0) { StopCharging(); return; }

        GameObject held = holdParent.GetChild(0).gameObject;
        if (held == null) { StopCharging(); return; }

        // desparentar y preparar Rigidbody
        held.transform.SetParent(null, true);

        Rigidbody rb = held.GetComponent<Rigidbody>();
        if (rb == null) rb = held.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // ============================
        //  ORIGEN Y DIRECCIÓN ARREGLADA
        // ============================
        Vector3 aimOrigin = held.transform.position;
        Vector3 aimDirection;

        if (useCameraForward && cameraTransform != null)
        {
            aimDirection = cameraTransform.forward.normalized;
        }
        else if (holdParent != null)
        {
            aimDirection = holdParent.forward.normalized;
        }
        else
        {
            aimDirection = transform.forward.normalized;
        }

        // eje lateral
        Vector3 axisRight;
        if (useCameraForward && cameraTransform != null)
            axisRight = cameraTransform.right.normalized;
        else if (holdParent != null)
            axisRight = holdParent.right.normalized;
        else
            axisRight = transform.right.normalized;

        // comprobamos si está invertido (apunta hacia dentro)
        Vector3 fromHolderToHeld;
        if (holdParent != null)
            fromHolderToHeld = (held.transform.position - holdParent.position);
        else
            fromHolderToHeld = (held.transform.position - transform.position);

        if (fromHolderToHeld.sqrMagnitude > 1e-6f)
        {
            Vector3 dirToHeld = fromHolderToHeld.normalized;
            if (Vector3.Dot(aimDirection, dirToHeld) < 0f)
            {
                aimDirection = -aimDirection;
                if (drawDebug) Debug.Log("[ThrowObject] aimDirection invertida automáticamente (dot<0).");
            }
        }

        if (axisRight.sqrMagnitude < 1e-6f)
        {
            axisRight = Vector3.Cross(Vector3.up, aimDirection).normalized;
            if (axisRight.sqrMagnitude < 1e-6f) axisRight = Vector3.right;
        }

        if (drawDebug)
        {
            Debug.Log($"[ThrowObject] aimOrigin={aimOrigin:F3} aimDir={aimDirection:F3} axisRight={axisRight:F3} invert={invertThrowDirection}");
        }
        // ============================


        // ----- Modos de lanzamiento -----
        if (throwMode == ThrowMode.ImpulseOld)
        {
            Vector3 dir = (aimDirection + Vector3.up * 0.12f).normalized;
            if (invertThrowDirection) dir = -dir;
            rb.AddForce(dir * speed, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * extraTorque, ForceMode.Impulse);
            if (drawDebug) Debug.DrawRay(aimOrigin, dir * 2f, Color.yellow, 2f);
        }
        else if (throwMode == ThrowMode.ParabolicAngle)
        {
            Vector3 tiltedDir = TiltDirectionByAxis(aimDirection, fixedAngleDegrees, axisRight);
            if (invertThrowDirection) tiltedDir = -tiltedDir;
            rb.velocity = tiltedDir * speed;
            rb.AddTorque(Random.onUnitSphere * extraTorque, ForceMode.Impulse);
            if (drawDebug) Debug.DrawRay(aimOrigin, tiltedDir * 2f, Color.cyan, 2f);
        }
        else if (throwMode == ThrowMode.ParabolicToDistance)
        {
            Vector3 targetPoint;
            RaycastHit hit;
            if (Physics.Raycast(aimOrigin, aimDirection, out hit, Mathf.Max(raycastMaxDistance, targetDistance)))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = aimOrigin + aimDirection * targetDistance;
            }

            bool ok;
            Vector3 velocity = CalculateVelocityToTarget(held.transform.position, targetPoint, fixedAngleDegrees, out ok);
            if (ok)
            {
                float baseSpeed = velocity.magnitude;
                if (baseSpeed < 0.001f) baseSpeed = 1f;
                Vector3 finalVel = velocity * (speed / baseSpeed);
                if (invertThrowDirection) finalVel = -finalVel;
                rb.velocity = finalVel;
                if (drawDebug) Debug.DrawRay(aimOrigin, (targetPoint - held.transform.position).normalized * 2f, Color.green, 2f);
            }
            else
            {
                Vector3 tiltedFallback = TiltDirectionByAxis(aimDirection, fixedAngleDegrees, axisRight);
                if (invertThrowDirection) tiltedFallback = -tiltedFallback;
                rb.velocity = tiltedFallback * speed;
                if (drawDebug) Debug.DrawRay(aimOrigin, tiltedFallback * 2f, Color.magenta, 2f);
            }
            rb.AddTorque(Random.onUnitSphere * extraTorque, ForceMode.Impulse);
        }

        StopCharging();
    }

    // Rota 'dir' alrededor del eje 'axisRight' por angleDegrees (subida).
    Vector3 TiltDirectionByAxis(Vector3 dir, float angleDegrees, Vector3 axisRight)
    {
        Vector3 forward = dir.normalized;
        if (axisRight.sqrMagnitude < 1e-6f)
        {
            axisRight = Vector3.Cross(Vector3.up, forward).normalized;
            if (axisRight.sqrMagnitude < 1e-6f) axisRight = Vector3.right;
        }
        Quaternion q = Quaternion.AngleAxis(-angleDegrees, axisRight);
        Vector3 tilted = q * forward;
        return tilted.normalized;
    }

    // Calcula velocidad inicial para alcanzar target con un ángulo dado (en grados).
    Vector3 CalculateVelocityToTarget(Vector3 start, Vector3 target, float angleDegrees, out bool ok)
    {
        ok = false;
        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float x = toTargetXZ.magnitude;
        float y = toTarget.y;

        if (x < 0.0001f) { ok = false; return Vector3.zero; }

        float angle = angleDegrees * Mathf.Deg2Rad;
        float g = -Physics.gravity.y; // gravedad positiva

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float denom = 2f * cos * cos * (x * Mathf.Tan(angle) - y);
        if (denom <= 0f) { ok = false; return Vector3.zero; }

        float v2 = g * x * x / denom;
        if (v2 <= 0f) { ok = false; return Vector3.zero; }

        float v = Mathf.Sqrt(v2);
        Vector3 dirXZ = toTargetXZ.normalized;
        Vector3 result = dirXZ * (v * cos) + Vector3.up * (v * sin);
        ok = true;
        return result;
    }
}
