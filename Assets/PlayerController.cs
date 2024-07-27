using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float m_Speed;

    void Start()
    {
        m_Speed = 1f;
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
    }
}
