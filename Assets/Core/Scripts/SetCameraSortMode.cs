using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraSortMode : MonoBehaviour
{
    public TransparencySortMode transparencySortMode;

    void Start()
    {
        var camera = GetComponent<Camera>();
        if (camera != null)
            camera.transparencySortMode = transparencySortMode;
        else
            Debug.LogError("SetCameraMode(" + gameObject.name + "): Must have camera component on same object");
    }
}
