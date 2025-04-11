using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static DamageDealerParameters;


public class PolygonCollider : MonoBehaviour
{

    public Polygon Polygon;
    private Vector2[] _prevPolygonPoints;

    [Header("Collision Settings")]
    public float ColliderWidth;

    public bool IsDamageDealer;

    [ShowIf("IsDamageDealer")]
    public float Damage;

    [ShowIf("IsDamageDealer")]
    public DamageDealerParameters.damageType DamageType;

    [Header("Save Settings")]
    public string SavePath;

    public bool autoSave = true;

    [Button(null, EButtonEnableMode.Editor)]
    public void GenerateCollision()
    {
        if (transform.Find("Collision") != null) DestroyImmediate(transform.Find("Collision").gameObject);

        if (transform.Find("TerrainDamageDealer") != null) DestroyImmediate(transform.Find("TerrainDamageDealer").gameObject);

        if (IsDamageDealer)
        {
            GameObject DamageDealerCondition = new GameObject("TerrainDamageDealer");

            DamageDealerCondition.transform.parent = transform;


            GameObject DamageDealerAmount = new GameObject("DamageAmount");

            DamageDealerAmount.transform.parent = DamageDealerCondition.transform;

            GameObject DamageAmountValue = new GameObject(Damage.ToString());

            DamageAmountValue.transform.parent = DamageDealerAmount.transform;


            GameObject DamageDealerType = new GameObject("DamageType");

            DamageDealerType.transform.parent = DamageDealerCondition.transform;

            GameObject DamageTypeValue = new GameObject(Enum.GetName(typeof(damageType), DamageType));

            DamageTypeValue.transform.parent = DamageDealerType.transform;
        }


        GameObject ColliderGameObject = new GameObject("Collision");

        ColliderGameObject.layer = 10;

        ColliderGameObject.transform.parent = transform;

        ColliderGameObject.transform.position = transform.position;

        List<MeshFilter> ColliderMeshes = new List<MeshFilter>();

        for (int y = 0; y < Polygon.Points.ToArray().Length; y++)
        {
            if (y == Polygon.Points.ToArray().Length - 1)
            {
                var EndQuad = InstanciateCollisionQuad(Polygon.Points[y], Polygon.Points[0], ColliderGameObject.transform, ColliderWidth);

                ColliderMeshes.Add(EndQuad.GetComponent<MeshFilter>());

                break;
            }

            var Quad = InstanciateCollisionQuad(Polygon.Points[y], Polygon.Points[y + 1], ColliderGameObject.transform, ColliderWidth);

            ColliderMeshes.Add(Quad.GetComponent<MeshFilter>());
        }

        CombineInstance[] combine = new CombineInstance[ColliderMeshes.ToArray().Length];

        int z = 0;
        while (z < ColliderMeshes.ToArray().Length)
        {
            combine[z].mesh = ColliderMeshes[z].sharedMesh;
            combine[z].transform = ColliderMeshes[z].transform.localToWorldMatrix;
            ColliderMeshes[z].gameObject.SetActive(false);

            z++;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        ColliderGameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        ColliderGameObject.AddComponent<MeshCollider>();

        foreach (MeshFilter collider in ColliderMeshes)
        {
            DestroyImmediate(collider.gameObject);
        }

        GenerateSavePath();

#if UNITY_EDITOR
        AssetDatabase.CreateAsset(mesh, SavePath);
        AssetDatabase.SaveAssets();
#endif
    }

    [Button(null, EButtonEnableMode.Editor)]
    public void RemoveCollision()
    {
        if (transform.Find("TerrainDamageDealer") != null) DestroyImmediate(transform.Find("TerrainDamageDealer").gameObject);

        if (transform.Find("Collision") != null)
        {
            DestroyImmediate(transform.Find("Collision").gameObject);

#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(SavePath);
            AssetDatabase.SaveAssets();
#endif
        }
    }

    [Button("Generate Path", EButtonEnableMode.Editor)]
    void RegeneratePath()
    {
        SavePath = string.Empty;
        GenerateSavePath();
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

    public GameObject InstanciateCollisionQuad(Vector2 pointA, Vector2 pointB, Transform Parent, float QuadWidth)
    {
        Vector2 midpoint = (pointA + pointB) / 2f;

        float distance = Vector2.Distance(pointA, pointB);

        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

        quad.transform.position = new Vector3(midpoint.x, midpoint.y, 0);

        quad.transform.rotation = Quaternion.Euler(90 + angle + 180, -90, 90);

        quad.transform.localScale = new Vector3(distance, 1, 1);

        quad.transform.localScale = new Vector3(quad.transform.localScale.x, QuadWidth, quad.transform.localScale.z);

        DestroyImmediate(quad.GetComponent<MeshRenderer>());

        quad.transform.parent = Parent;

        return quad;
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (autoSave && Tools.current != Tool.None && Polygon != null)
        {
            if (_prevPolygonPoints == null)
            {
                _prevPolygonPoints = new Vector2[Polygon.Points.Count()];
                for (int i = 0; i < Polygon.Points.Count(); ++i)
                    _prevPolygonPoints[i] = Polygon.Points[i];
            }
            else if (Polygon.Points.Count() != _prevPolygonPoints.Count())
            {
                _prevPolygonPoints = new Vector2[Polygon.Points.Count()];
                for (int i = 0; i < Polygon.Points.Count(); ++i)
                    _prevPolygonPoints[i] = Polygon.Points[i];
                Debug.Log(gameObject.name + " point count changed");
                GenerateCollision();
            }
            else
            {
                for (int i = 0; i < _prevPolygonPoints.Count(); ++i)
                {
                    if (_prevPolygonPoints[i] != Polygon.Points[i])
                    {
                        for (int j = 0; j < Polygon.Points.Count(); ++j)
                            _prevPolygonPoints[j] = Polygon.Points[j];
                        Debug.Log(gameObject.name + " points changed");
                        GenerateCollision();

                        break;
                    }
                }
            }
        }
#endif
    }
}
