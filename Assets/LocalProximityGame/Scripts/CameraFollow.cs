using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform m_target;
    public float m_smoothSpeed = .125f;
    public bool m_lookAt = false;
    private Vector3 _offset;
    private void Awake()
    {
        _offset = transform.position;
    }
    private void FixedUpdate()
    {
        Vector3 desiredPosition = m_target.position + _offset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, m_smoothSpeed);

        transform.position = smoothPosition;
        if(m_lookAt)
            transform.LookAt(m_target);

    }


}