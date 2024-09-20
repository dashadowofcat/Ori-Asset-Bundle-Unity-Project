using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Polygon : MonoBehaviour
{
    public List<Vector2> Points;

    [Header("Gizmos")]
    public float SelectionPointSize = .6f;
    public Color SelectionPointColor = Color.white;
    public Color LineColor = Color.green;

    public bool ShowAddPoints;

    public Color AddPointColor = Color .green;
    public float AddPointSize = .3f;
}

#if UNITY_EDITOR

[CustomEditor(typeof(Polygon))]
public class PolygonEditor : Editor
{
    public void OnSceneGUI()
    {
        Polygon polygon = (Polygon)target;

        Handles.color = new Color(1, 1, 1, 0);

        Vector3 snap = Vector3.one * 0.5f;

        for (int x = 0; x < polygon.Points.ToArray().Length; x++)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 NewPoint = Handles.FreeMoveHandle((Vector3)polygon.Points[x] + polygon.transform.position, new Quaternion(), polygon.SelectionPointSize, snap, Handles.RectangleHandleCap) - polygon.transform.position;

            if (EditorGUI.EndChangeCheck())
            {
                polygon.Points[x] = new Vector2(NewPoint.x, NewPoint.y);
            }
        }


        Handles.color = polygon.SelectionPointColor;

        for (int i = 0; i < polygon.Points.ToArray().Length; i++)
        {
            Undo.RecordObject(polygon, "Change Polygon Point Position");

            Handles.SphereHandleCap(0, (Vector3)polygon.Points[i] + polygon.transform.position, polygon.transform.rotation, polygon.SelectionPointSize, EventType.Repaint);
        }


        Handles.color = polygon.LineColor;

        for (int y = 0; y < polygon.Points.Count; y++)
        {
            Vector2 point1;
            Vector2 point2;

            if (y < polygon.Points.Count - 1)
            {
                point1 = polygon.Points[y] + (Vector2)polygon.transform.position;
                point2 = polygon.Points[y + 1] + (Vector2)polygon.transform.position;
            }
            else
            {
                point1 = polygon.Points[y] + (Vector2)polygon.transform.position;
                point2 = polygon.Points[0] + (Vector2)polygon.transform.position;
            }

            Handles.DrawLine(point1, point2);
        }

        if (!polygon.ShowAddPoints) return;

        Handles.color = polygon.AddPointColor;

        for (int y = 0; y < polygon.Points.Count; y++)
        {
            Vector2 point1 = polygon.Points[y] + (Vector2)polygon.transform.position;
            Vector2 point2 = (y < polygon.Points.Count - 1) ? polygon.Points[y + 1] + (Vector2)polygon.transform.position : polygon.Points[0] + (Vector2)polygon.transform.position;

            Quaternion rotation = Quaternion.identity;

            rotation.eulerAngles = new Vector3(0,0,45);

            Vector2 midPoint = (point1 + point2) / 2;
            if (Handles.Button(midPoint, rotation, polygon.AddPointSize, polygon.AddPointSize / 2, Handles.RectangleHandleCap))
            {
                Undo.RecordObject(polygon, "Add Polygon Point");
                polygon.Points.Insert(y + 1, midPoint - (Vector2)polygon.transform.position);
                break;
            }
        }

    }
}

#endif