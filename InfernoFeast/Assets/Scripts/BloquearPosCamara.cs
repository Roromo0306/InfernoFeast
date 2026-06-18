using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloquearPosCamara : MonoBehaviour
{
    public float minX = -14.48506f;

    public float minZ = -13.03484f;
    public float maxZ = 11.90381f;

    void OnEnable()
    {
        Camera.onPreCull += ClampCamera;
    }

    void OnDisable()
    {
        Camera.onPreCull -= ClampCamera;
    }

    void ClampCamera(Camera cam)
    {
        if (cam != GetComponent<Camera>()) return;

        Vector3 pos = transform.position;

        pos.x = Mathf.Max(pos.x, minX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        transform.position = pos;
    }
}
