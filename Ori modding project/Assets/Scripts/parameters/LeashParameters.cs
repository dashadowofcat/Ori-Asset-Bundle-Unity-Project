using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LeashParameters : ElementParameter
{
    public bool IsSticky;

    void Update()
    {
        SetBool("Sticky", IsSticky);
    }
}
