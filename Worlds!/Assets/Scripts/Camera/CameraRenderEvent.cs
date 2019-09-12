using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CameraEventHandler(Camera camera);

public abstract class CameraRenderEvent : MonoBehaviour
{
    protected Camera m_camera { get; private set; }

    void Start()
    {
        m_camera = GetComponent<Camera>();
    }
}
