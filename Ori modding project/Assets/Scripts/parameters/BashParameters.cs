using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BashParameters : ElementParameter
{
    public bool hasParticles;

    void Update()
    {
        SetBool("HasParticles", hasParticles);
    }
}
