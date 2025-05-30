using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    public Color anchorColor = Color.white;
    public Color controlColor = Color.red;
    public Color segmentColor = Color.green;
    public Color selectedSegmentColor = Color.yellow;
    public float anchorDiameter = 0.5f;
    public float controlDiameter = 0.25f;
    public bool displayControlPoints = true;

    public void CreatePath()
    {
        path = new Path(Vector3.zero);
    }
    
}
