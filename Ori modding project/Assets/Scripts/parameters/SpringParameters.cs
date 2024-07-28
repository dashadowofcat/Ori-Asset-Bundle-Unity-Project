using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpringParameters : ElementParameter
{
    public float SpringHeight;

    void Update()
    {
        SetFloat("Force", SpringHeight);
    }
}
