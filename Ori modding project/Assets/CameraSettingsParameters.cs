using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraSettingsParameters : ElementParameter
{
    public Vector3 position;
    public float fieldOfView;

    void Update()
    {
        SetFloat("X", position.x);
        SetFloat("Y", position.y);
        SetFloat("Z", position.z);

        SetFloat("FoV", fieldOfView);
    }
}
