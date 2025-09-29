using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Atributos del Player")]
    public float speed = 5f;

    private float MoveX, MoveZ;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveX = Input.GetAxis("Vertical");
        MoveZ = Input.GetAxis("Horizontal");

        Vector3 movimiento = new Vector3(MoveX, 0, MoveZ).normalized;
        rb.velocity = movimiento*speed;
    }
}
