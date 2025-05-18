using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathMeshCreator))]
public class PathMeshEditor : Editor
{
    PathMeshCreator creator;

    private void OnSceneGUI()
    {
        if(creator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creator.UpdatePath();
        }
    }

    void OnEnable()
    {
        creator = (PathMeshCreator)target;
    }
}
