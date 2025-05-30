using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;
using static DamageDealerParameters;

[ExecuteAlways]

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class PathMeshCreator : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 0.5f;
    public float pathWidth = 0.25f;
    public bool autoUpdate = false;
    public float tiling = 1;
    public bool hasRenderer = true;
    public bool hasCollision = false;


    public bool IsDamageDealer;

    [ShowIf("IsDamageDealer")]
    public float Damage;

    [ShowIf("IsDamageDealer")]
    public DamageDealerParameters.damageType DamageType;

    [Header("Save Settings")]
    public string SavePath = string.Empty;

    public bool autoSave = true;

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
        {
            savePathAsset();
        }
    }

    public void UpdatePath()
    {
        if(SavePath == string.Empty)
        {
            GenerateSavePath();
        }
        Path path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing, 1);
        if(hasRenderer)
        {
            GetComponent<MeshFilter>().mesh = CreatePathMesh(points, path.IsClosed);
            int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat); // THIS NEEDS TO BE FIXED
        }
        if (hasCollision)
        {
            GetComponent<MeshCollider>().sharedMesh = CreatePathCollision(points, path.IsClosed);
        }
    }

    Mesh CreatePathMesh(Vector3[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] tris = new int[2 * numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if ( i < points.Length - 1 || isClosed) // Not the last point
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }

            if(i > 0 || isClosed) // Not the first point
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }

            forward.Normalize();
            Vector3 top = new Vector3(-forward.y, forward.x, 0);
            top.Normalize();

            verts[vertIndex] = points[i];
            verts[vertIndex + 1] = points[i] - new Vector3(top.x, top.y, points[i].z + pathWidth);

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if(i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }

    Mesh CreatePathCollision(Vector3[] points, bool isClosed) 
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] tris = new int[2 * numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1 || isClosed) // Not the last point
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }

            if (i > 0 || isClosed) // Not the first point
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }

            forward.Normalize();
            Vector3 top = new Vector3(-forward.y, forward.x, 0);
            top.Normalize();

            verts[vertIndex] = points[i] + new Vector3(0, 0, pathWidth);
            verts[vertIndex + 1] = points[i] - new Vector3(0, 0, pathWidth);

            if (i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;


        return mesh;
    }

    void savePathAsset()
    {
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(GetComponent<MeshCollider>().sharedMesh, SavePath);
        AssetDatabase.CreateAsset(GetComponent<MeshFilter>(), SavePath);
        AssetDatabase.SaveAssets();
#endif
    }

    void GenerateSavePath()
    {
        if (SavePath == string.Empty)
        {
            SavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(5)}.mesh";

#if UNITY_EDITOR
            while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(SavePath)))
            {
                SavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(5)}.mesh";
            }
#endif
        }
    }

    string GenerateRandomString(int Length)
    {
        System.Random random = new System.Random();

        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return new string(Enumerable.Repeat(chars, Length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
