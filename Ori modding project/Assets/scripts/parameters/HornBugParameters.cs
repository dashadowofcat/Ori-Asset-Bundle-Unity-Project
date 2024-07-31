using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[ExecuteInEditMode]

public class HornBugParameter : ElementParameter
{
    [InfoBox("The health of the enemy", EInfoBoxType.Normal)]
    public int Health;

    // Update is called once per frame
    void Update()
    {
        SetInt("Health", Health);
    }
}
