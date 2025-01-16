using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]
public class LifePlantParameters : ElementParameter
{
    public int NumberOfHealthOrbs;
    
    void Update()
    {
        SetInt("NumberOfHealthOrbs", NumberOfHealthOrbs);
    }
}
