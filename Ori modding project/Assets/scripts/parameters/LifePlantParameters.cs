using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]

public class LifePlantParameters : ElementParameter
{
    [InfoBox("The max amount of orbs created based on current health", EInfoBoxType.Normal)]
    public int IdealOrbs;
    [InfoBox("Will overwrite dynamic orb scaling based on current health if > 0", EInfoBoxType.Normal)]
    public int NumberOfHealthOrbs;
    
    // Update is called once per frame
    void Update()
    {
        SetInt("IdealOrbs", IdealOrbs);
        SetInt("NumberOfHealthOrbs", NumberOfHealthOrbs);

    }
}
