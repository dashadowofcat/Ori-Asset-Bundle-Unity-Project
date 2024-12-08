using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]
public class LifePlantParameters : ElementParameter
{
    public int NumberOfHealthOrbs;
    
    // Update is called once per frame
    void Update()
    {
        SetInt("NumberOfHealthOrbs", NumberOfHealthOrbs);

    }
}
