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
        EventType curEventType = Event.current.type;

        //if(curEventType != EventType.Layout && curEventType != EventType.MouseMove)
        //{
        //    Debug.Log("Event Type: " + curEventType.ToString());
        //}

        if(creator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creator.UpdatePath();
        }
    }
}
