using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementParameter : MonoBehaviour
{

    public void SetString(string PropertyName, string Value)
    {
        transform.Find(PropertyName).GetChild(0).name = Value;
    }

    public void SetInt(string PropertyName, int Value)
    {
        transform.Find(PropertyName).GetChild(0).name = Value.ToString();
    }

    public void SetFloat(string PropertyName, float Value)
    {
        transform.Find(PropertyName).GetChild(0).name = Value.ToString();
    }

    public void SetBool(string PropertyName, bool Value)
    {
        transform.Find(PropertyName).GetChild(0).name = Value.ToString().ToLower();
    }
}
