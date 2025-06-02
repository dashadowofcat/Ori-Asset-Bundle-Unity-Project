using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

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
                    ConvertTo3DQuad(sprite.texture, mousePosition);
                    Event.Use();
                }
                else if(draggedObject is Texture2D texture2D)
                {
                    Vector3 mousePosition = HandleUtility.GUIPointToWorldRay(Event.mousePosition).origin;
                    ConvertTo3DQuad(texture2D, mousePosition);
                    Event.Use();
                }
            }
            
        }
        
    }

    private static void ConvertTo3DQuad(Texture2D texture, Vector3 position)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);

        if (PrefabStageUtility.GetCurrentPrefabStage() != null) plane.transform.parent = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.transform;

        Renderer renderer = plane.GetComponent<Renderer>();

        DestroyImmediate(plane.GetComponent<MeshCollider>());

        Material spriteMaterial = new Material(Shader.Find("Sprites/Default"));
        spriteMaterial.mainTexture = texture;

        string materialFolderPath = "Assets/level data/Resources/materials";
        string materialPath = $"{materialFolderPath}/{texture.name} ({plane.GetInstanceID()})_Material.mat";
        AssetDatabase.CreateAsset(spriteMaterial, materialPath);
        AssetDatabase.SaveAssets();

        renderer.sharedMaterial = spriteMaterial;

        position.z = 0;

        plane.transform.position = position;
        plane.transform.localScale = new Vector3(texture.width / 100f, texture.height / 100f, 1);
        plane.name = texture.name;

        Selection.activeGameObject = plane;
    }
}
