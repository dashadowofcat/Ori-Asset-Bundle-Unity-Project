using NaughtyAttributes.Editor;
using NaughtyAttributes;
using NaughtyAttributes.Test;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;

public class ElementsWindow : EditorWindow
{
    [MenuItem("Window/Elements")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<ElementsWindow>("Element Selector");

        window.maxSize = new Vector2(179, 595);
        window.minSize = window.maxSize;

        Setup();
    }

    private static GameObject[] prefabs;

    void OnEnable()
    {
        Setup();
    }

    private static void Setup()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/level data/elements" });
        prefabs = new GameObject[prefabGUIDs.Length];

        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }

    private void OnGUI()
    {
        DrawPrefabs();

        HandlePrefabSelection();

        HandleSelectionHighlight();
    }

    GameObject currentlySelectedPrefab;


    
    
    Vector2 scrollPos;

    private void DrawPrefabs()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(64 * 4), GUILayout.Height((64 + 10) * 8));

        int itemsPerRow = 2;
        int itemCount = 0;

        GUILayout.BeginHorizontal();

        foreach (GameObject prefab in prefabs)
        {
            GUILayout.BeginVertical(GUILayout.Width(64 + 32));

            Rect rect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));

            if (prefabRectMap == null) prefabRectMap = new Dictionary<Rect, GameObject>();
            if (rectPrefabMap == null) rectPrefabMap = new Dictionary<GameObject, Rect>();

            prefabRectMap[rect] = prefab;
            rectPrefabMap[prefab] = rect;


            Texture2D preview = AssetPreview.GetAssetPreview(prefab);

            if (preview != null)
            {
                EditorGUI.DrawPreviewTexture(rect, preview);
            }
            else
            {
                EditorGUI.DrawTextureTransparent(rect, EditorGUIUtility.FindTexture("Prefab Icon"));
            }

            GUIStyle style = new GUIStyle(GUI.skin.label);

            GUILayout.Label(GetCutoffName(prefab.name, 14), style);


            GUILayout.EndVertical();

            itemCount++;

            if (itemCount % itemsPerRow == 0)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }

            
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private Dictionary<Rect, GameObject> prefabRectMap;
    private Dictionary<GameObject, Rect> rectPrefabMap;

    private void HandlePrefabSelection()
    {
        Vector2 mousePosition = Event.current.mousePosition;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            foreach (var kvp in prefabRectMap)
            {
                Rect rect = kvp.Key;

                rect.position -= scrollPos;

                if (rect.Contains(mousePosition))
                {
                    GameObject clickedPrefab = kvp.Value;

                    currentlySelectedPrefab = clickedPrefab;

                    DragAndDrop.PrepareStartDrag();

                    DragAndDrop.objectReferences = new Object[] { clickedPrefab };

                    DragAndDrop.StartDrag(clickedPrefab.name);

                    break;
                } 
            }
        }
    }


    private void HandleSelectionHighlight()
    {
        if (currentlySelectedPrefab == null) return;

        Rect rect = rectPrefabMap[currentlySelectedPrefab];

        rect.y -= 1;
        rect.x -= 1;

        rect.position -= scrollPos;

        EditorGUI.DrawRect(rect, new Color(0.227f, 0.475f, 0.833f, 0.2f));
    }


    public string GetCutoffName(string value, int Cutoff)
    {
        if(value.Length > Cutoff)
        {
            value = value.Remove(Cutoff - 3);

            value += "...";

            return value;
        }

        return value;
    }

}
