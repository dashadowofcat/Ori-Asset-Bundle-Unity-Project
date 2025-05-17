using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GoalTextParemeters : ElementParameter
{
    public string GoalText;

    void Update()
    {
        SetString("Level Complete!", GoalText);
    }
}
