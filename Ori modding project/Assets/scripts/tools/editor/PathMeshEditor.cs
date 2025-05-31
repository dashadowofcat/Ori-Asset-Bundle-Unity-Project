using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathMeshCreator))]
public class PathMeshEditor : Editor
{
    PathMeshCreator creator;

    void OnEnable()
    {
        creator = (PathMeshCreator)target;
    }

    private void OnSceneGUI()
    {
        if(creator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creator.UpdatePath();
        }
    }
}
