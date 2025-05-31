using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path path;

    const float segmentSelectDistanceThreshold = 0.1f;
    int selectedSegmentIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create New"))
        {
            Undo.RecordObject(creator, "Create new path");
            creator.CreatePath();
            path = creator.path;
        }

        bool isClosed = GUILayout.Toggle(path.IsClosed, "Closed");
        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(creator, "Toggle closed path");
            path.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator, "Toggle auto set controls");
            path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if(selectedSegmentIndex != -1) // If hovering over a segment
            {
                Undo.RecordObject(creator, "Split segment");
                path.SplitSegment(new Vector3(mousePos.x, mousePos.y, creator.transform.position.z) - creator.transform.position, selectedSegmentIndex);
            }
            else if(!path.IsClosed)
            {
                Undo.RecordObject(creator, "Add segment");
                path.AddSegment(new Vector3(mousePos.x, mousePos.y, creator.transform.position.z) - creator.transform.position);
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.shift)
        {
            if(selectedSegmentIndex != -1 && !path.IsClosed)
            {
                Undo.RecordObject(creator, "Straighten segment");
                path.StraightenSegment(selectedSegmentIndex);
            }
            else if (selectedSegmentIndex != -1 && selectedSegmentIndex != path.NumSegments - 1)
            {
                Undo.RecordObject(creator, "Straighten segment");
                path.StraightenSegment(selectedSegmentIndex);
            }
            else if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(creator, "Straighten segment");
                path.StraightenClosedSegment();
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistToAnchor = creator.anchorDiameter * 0.5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < path.NumPoints; i += 3)
            {
                float dist = Vector2.Distance(mousePos, path[i]);
                if (dist < minDistToAnchor)
                {
                    minDistToAnchor = dist;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(creator, "Delete segment");
                path.DeleteSegment(closestAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minDistToSegment = segmentSelectDistanceThreshold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < path.NumSegments; i++)
            {
                Vector3[] points = path.GetPointsInSegment(i);

                float dist = HandleUtility.DistancePointBezier(new Vector3(mousePos.x, mousePos.y, creator.transform.position.z) - creator.transform.position, points[0], points[3], points[1], points[2]);
                if (dist < minDistToSegment)
                {
                    minDistToSegment = dist;
                    newSelectedSegmentIndex = i;
                }
            }

            if(newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        path.controlHeld = guiEvent.control;

        HandleUtility.AddDefaultControl(0);
    }

    void Draw()
    {
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            for(int j = 0; j < points.Length; ++j)
            {
                points[j] += creator.transform.position;
            }

            if(creator.displayControlPoints)
            {
                Handles.color = Color.red;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }

            Color segmentColor = (i == selectedSegmentIndex && Event.current.shift) ? creator.selectedSegmentColor : creator.segmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 10);
        }

        for (int i = 0; i < path.NumPoints; i++)
        {
            if (i % 3 == 0 || creator.displayControlPoints)
            {
                Handles.color = (i % 3 == 0) ? creator.anchorColor : creator.controlColor;
                float handleSize = (i % 3 == 0) ? creator.anchorDiameter : creator.controlDiameter;

                Vector3 newPos = Handles.FreeMoveHandle(path[i] + creator.transform.position, Quaternion.identity, handleSize, Vector2.zero, Handles.CylinderHandleCap);
                if (path[i] + creator.transform.position != newPos)
                {
                    Undo.RecordObject(creator, "Move Point");
                    path.MovePoint(i, newPos - creator.transform.position);
                }
            }
        }
    }

    private void OnEnable()
    {
        creator = (PathCreator)target;

        if (creator.path == null)
        {
            creator.CreatePath();
        }

        path = creator.path;
    }
 }
