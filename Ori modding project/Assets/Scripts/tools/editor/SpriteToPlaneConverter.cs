using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteToPlaneConverter : EditorWindow
{
    [MenuItem("Window/Sprite to Plane Converter")]
    public static void ShowWindow()
    {
        GetWindow<SpriteToPlaneConverter>("Sprite to Plane Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag a sprite here to create a 3D plane with the sprite texture.", EditorStyles.wordWrappedLabel);
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Sprite Here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Texture2D texture)
                        {
                            ConvertTo3DPlane(texture);
                        }
                        else if (draggedObject is Sprite sprite)
                        {
                            ConvertTo3DPlane(sprite.texture);
                        }
                    }
                }
                break;
        }
    }

    private void ConvertTo3DPlane(Texture2D texture)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Renderer renderer = plane.GetComponent<Renderer>();

        DestroyImmediate(plane.GetComponent<MeshCollider>());

        Material spriteMaterial = new Material(Shader.Find("Sprites/Default"));
        spriteMaterial.mainTexture = texture;

        string materialFolderPath = "Assets/level data/cache/materials";
        string materialPath = $"{materialFolderPath}/{texture.name} ({plane.GetInstanceID()})_Material.mat";
        AssetDatabase.CreateAsset(spriteMaterial, materialPath);
        AssetDatabase.SaveAssets();

        renderer.sharedMaterial = spriteMaterial;

        plane.transform.localScale = new Vector3(texture.width / 1000f, 1, texture.height / 1000f);
        plane.transform.eulerAngles = new Vector3(90, 0, 180);

        plane.name = texture.name;

        Selection.activeGameObject = plane;
    }
}