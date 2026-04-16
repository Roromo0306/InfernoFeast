using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CongelarCamara : MonoBehaviour
{
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;
    public bool freezePosition = true;
    public bool freezeRotation = true;

    void Start()
    {
        fixedPosition = transform.position;
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (freezePosition)
            transform.position = fixedPosition;

        if (freezeRotation)
            transform.rotation = fixedRotation;
    }
}
