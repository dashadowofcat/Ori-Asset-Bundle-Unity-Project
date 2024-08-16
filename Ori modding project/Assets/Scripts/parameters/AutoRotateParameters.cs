using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AutoRotateParameters : ElementParameter
{
    public float RotateSpeed;

    void Update()
    {
        SetFloat("Speed", RotateSpeed);
    }
}
