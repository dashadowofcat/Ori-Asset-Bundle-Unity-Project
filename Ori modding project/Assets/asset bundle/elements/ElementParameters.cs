using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class ElementParameters : MonoBehaviour
{
    [System.Serializable]
    public struct Parameter
    {
        [HideInInspector]
        public string Name;

        public string Value;

        public Parameter(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }

    public List<Parameter> Parameters = new List<Parameter>();

    void Reset()
    {
        foreach (Transform child in transform)
        {
            Parameters.Add(new Parameter(child.name, child.GetChild(0).name));
        }
    }

    void Update()
    {
        foreach (Parameter parameter in Parameters)
        {
            foreach (Transform child in transform)
            {
                if(child.name == parameter.Name)
                {
                    child.GetChild(0).name = parameter.Value;
                }
            }
        }

        if(Parameters.Count > transform.childCount)
        {
            Parameters.Clear();

            foreach (Transform child in transform)
            {
                Parameters.Add(new Parameter(child.name, child.GetChild(0).name));
            }
        }
    }
}
