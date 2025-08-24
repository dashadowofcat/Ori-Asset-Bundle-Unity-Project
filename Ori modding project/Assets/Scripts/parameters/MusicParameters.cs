using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[ExecuteInEditMode]
public class MusicParameters : ElementParameter
{
    public InGameMusic inGameMusic;

    void Update()
    {
        SetString("InGame", Enum.GetName(typeof(InGameMusic), inGameMusic));
    }

    public enum InGameMusic
    {
        None,
        WellspringGlades,
        LumaPools
    }
}
