using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPostRender : CameraRenderEvent
{
    public CameraEventHandler d_event = delegate { };

    private void OnPostRender()
    {
        d_event(m_camera);
    }

    public static void AddEvent(Camera camera, CameraEventHandler postRenderEvent)
    {
        if(camera == null) throw new System.ArgumentException("Camera not found");

        CameraPostRender cameraPostRender = camera.GetComponent<CameraPostRender>();
        if(cameraPostRender == null) throw new System.ArgumentException("CameraPostRenderComponent not found");

        cameraPostRender.d_event += postRenderEvent;
    }

    public static void RemoveEvent(Camera camera, CameraEventHandler postRenderEvent)
    {
        if(camera == null) throw new System.ArgumentException("Camera not found");

        CameraPostRender cameraPostRender = camera.GetComponent<CameraPostRender>();
        if(cameraPostRender == null) throw new System.ArgumentException("CameraPostRenderComponent not found");

        cameraPostRender.d_event -= postRenderEvent;
    }
}
