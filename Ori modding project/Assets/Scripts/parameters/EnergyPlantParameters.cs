using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnergyPlantParameters : ElementParameter
{
    public int NumberOfEnergyOrbs;

    void Update()
    {
        SetInt("NumberOfEnergyOrbs", NumberOfEnergyOrbs);
    }
}
