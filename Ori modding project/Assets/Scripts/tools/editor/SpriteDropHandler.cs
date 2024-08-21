using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SpriteDropHandler : MonoBehaviour
{
    static SpriteDropHandler()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event Event = Event.current;

        if (Event.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is Sprite sprite)
                {

                    Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.mousePosition).origin;

                    ConvertTo3DPlane(sprite.texture, mousePosition);
                }
            }

            Event.Use();
        }
    }

    private static void ConvertTo3DPlane(Texture2D texture, Vector3 position)
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

        position.z = 0;

        plane.transform.position = position;

        plane.transform.localScale = new Vector3(texture.width / 1000f, 1, texture.height / 1000f);
        plane.transform.eulerAngles = new Vector3(90, 0, 180);

        plane.name = texture.name;

        Selection.activeGameObject = plane;
    }
}
