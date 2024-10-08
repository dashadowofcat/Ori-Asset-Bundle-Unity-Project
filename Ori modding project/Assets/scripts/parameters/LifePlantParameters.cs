using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]
public class LifePlantParameters : ElementParameter
{
    public int IdealOrbs;
    [InfoBox("Will overwrite dynamic orb scaling if larger than one", EInfoBoxType.Normal)]
    public int NumberOfHealthOrbs;
    
    // Update is called once per frame
    void Update()
    {
        SetInt("IdealOrbs", IdealOrbs);
        SetInt("NumberOfHealthOrbs", NumberOfHealthOrbs);

    }
}
