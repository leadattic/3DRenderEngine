using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float m_Speed;
    Rigidbody m_Rigidbody;

    public float m_Thrust = 200000f;
    void Start()
    {
        m_Speed = 1f;
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    void Update()
    {
        if (Input.GetKey("w"))
        {
            transform.position += transform.forward * m_Speed;
        }

        if (Input.GetKey("s"))
        {
            transform.position -= transform.forward * m_Speed;
        }

        if (Input.GetKey("a"))
        {
            transform.Rotate(0, -5, 0);
        }

        if (Input.GetKey("d"))
        {
            transform.Rotate(0, 5, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Rigidbody.AddForce(transform.up * m_Thrust);
        }
    }
}
