﻿using NaughtyAttributes;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static DamageDealerParameters;

[ExecuteAlways]

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class PathMeshCreator : MonoBehaviour, ILevelAsset
{
    public bool autoUpdate = true;

    [Range(0.05f, 1.5f)]
    public float spacing = 0.5f;

    [Header("Renderer Settings")]
    public bool hasRenderer = true;
    private Material rendererMaterial;
    public Color rendererColor = Color.white;
    public float rendererWidth = 0.05f;

    [Header("Collider Settings")]
    public bool hasCollision = true;
    public float colliderWidth = 1f;

    [Header("Damager Settings")]
    public bool IsDamageDealer;

    [ShowIf("IsDamageDealer")]
    public float Damage;

    [ShowIf("IsDamageDealer")]
    public DamageDealerParameters.damageType DamageType;

    [Header("Save Settings")]
    public string MaterialSavePath = string.Empty;
    public string ColliderMeshSavePath = string.Empty;
    public string RendererMeshSavePath = string.Empty;

    public void UpdatePath()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing, 1);

        if(hasRenderer)
        {
            if (rendererMaterial == null)
                rendererMaterial = new Material((Material)Resources.Load("Basic/White"));

            GetComponent<MeshRenderer>().material = rendererMaterial;
            GetComponent<MeshRenderer>().sharedMaterial.color = rendererColor;

            GetComponent<MeshFilter>().mesh = CreatePathMesh(points, path.IsClosed);
        }
        else
        {
            DeleteRendererAsset();

            GetComponent<MeshFilter>().mesh = null;
            GetComponent<MeshRenderer>().material = null;
        }

        if (hasCollision)
        {
            GetComponent<MeshCollider>().sharedMesh = CreatePathCollision(points, path.IsClosed);
        }
        else
        {
            DeleteColliderAsset();

            GetComponent<MeshCollider>().sharedMesh = null;
        }
    }

    private Mesh CreatePathMesh(Vector3[] points, bool isClosed)
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
            verts[vertIndex + 1] = points[i] - new Vector3(top.x, top.y, 0f) * rendererWidth;

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
        //mesh.uv = uvs;

        return mesh;
    }

    private Mesh CreatePathCollision(Vector3[] points, bool isClosed) 
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

            verts[vertIndex] = points[i] + new Vector3(0, 0, colliderWidth);
            verts[vertIndex + 1] = points[i] - new Vector3(0, 0, colliderWidth);

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

    public void DeleteRendererAsset()
    {
#if UNITY_EDITOR
        if (RendererMeshSavePath != string.Empty)
            AssetDatabase.DeleteAsset(RendererMeshSavePath);
        if (MaterialSavePath != string.Empty)
            AssetDatabase.DeleteAsset(MaterialSavePath);
#endif

        RendererMeshSavePath = string.Empty;
        MaterialSavePath = string.Empty;
    }

    public void DeleteColliderAsset()
    {
#if UNITY_EDITOR
        if (ColliderMeshSavePath != string.Empty)
            AssetDatabase.DeleteAsset(ColliderMeshSavePath);
#endif

        ColliderMeshSavePath = string.Empty;
    }

    public void SaveAsset()
    {
        DeleteColliderAsset();
        DeleteRendererAsset();

        UpdatePath();

        GenerateSavePath();

#if UNITY_EDITOR
        if(hasRenderer)
        {
            AssetDatabase.CreateAsset(GetComponent<MeshRenderer>().sharedMaterial, MaterialSavePath);
            AssetDatabase.CreateAsset(GetComponent<MeshFilter>().sharedMesh, RendererMeshSavePath);
        }
        
        if(hasCollision)
        {
            AssetDatabase.CreateAsset(GetComponent<MeshCollider>().sharedMesh, ColliderMeshSavePath);
        }

        AssetDatabase.SaveAssets();
#endif
    }

    private void GenerateSavePath()
    {
        if (MaterialSavePath == string.Empty)
        {
            MaterialSavePath = $"Assets/level data/Resources/materials/{transform.name} {GenerateRandomString(10)}.mat";

#if UNITY_EDITOR
            while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(MaterialSavePath)))
            {
                MaterialSavePath = $"Assets/level data/Resources/materials/{transform.name} {GenerateRandomString(10)}.mat";
            }
#endif
        }

        if (ColliderMeshSavePath == string.Empty)
        {
            ColliderMeshSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(10)}.mesh";

#if UNITY_EDITOR
            while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(ColliderMeshSavePath)) || ColliderMeshSavePath == RendererMeshSavePath)
            {
                ColliderMeshSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(10)}.mesh";
            }
#endif
        }

        if (RendererMeshSavePath == string.Empty)
        {
            RendererMeshSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(10)}.mesh";

#if UNITY_EDITOR
            while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(RendererMeshSavePath)) || ColliderMeshSavePath == RendererMeshSavePath)
            {
                RendererMeshSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(10)}.mesh";
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
