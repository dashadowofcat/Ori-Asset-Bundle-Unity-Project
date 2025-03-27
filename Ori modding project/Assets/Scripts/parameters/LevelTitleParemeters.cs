using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LevelTitleParemeters : ElementParameter
{
    public string Title;

    void Update()
    {
        SetString("Title", Title);
    }
}
