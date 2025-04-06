using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class PolygonRenderer : MonoBehaviour
{
    public Polygon Polygon;
    private Vector2[] _prevPolygonPoints;

    public Color Color;

    public Material Material;

    public string MaterialSavePath;
    public string MeshSavePath;

    public bool autoSave = true;

    [Button("Generate Renderer")]
    public void GenerateShape()
    {
        if (transform.Find("Renderer") != null) DestroyImmediate(transform.Find("Renderer").gameObject);


        List<Vector2> Points = Polygon.Points;

        Mesh PolygonMesh = PolygonMeshGenerator.CreateMeshFromPolygon(Points);


        GameObject Renderer = new GameObject("Renderer");

        Renderer.transform.position = transform.position;

        Renderer.transform.Rotate(new Vector3(-90, 0, 0));

        Renderer.transform.parent = gameObject.transform;

        MeshFilter meshFilter = Renderer.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = Renderer.AddComponent<MeshRenderer>();

        meshRenderer.material = new Material((Material)Resources.Load("Basic/White"));

        meshRenderer.sharedMaterial.color = Color;

        meshFilter.mesh = PolygonMesh;

        GenerateSavePath();

#if UNITY_EDITOR

        AssetDatabase.CreateAsset(meshRenderer.sharedMaterial, MaterialSavePath);

        AssetDatabase.CreateAsset(meshFilter.sharedMesh, MeshSavePath);

        AssetDatabase.SaveAssets();

#endif
    }

    

    [Button(null, EButtonEnableMode.Editor)]
    public void RemoveRenderer()
    {
        if (transform.Find("Renderer") != null)
        {
            DestroyImmediate(transform.Find("Renderer").gameObject);

#if UNITY_EDITOR
            AssetDatabase.DeleteAsset(MaterialSavePath);

            AssetDatabase.DeleteAsset(MeshSavePath);

            AssetDatabase.SaveAssets();
#endif
        }
    }

    [Button("Generate Path", EButtonEnableMode.Editor)]
    void RegeneratePath()
    {
        MaterialSavePath = string.Empty;
        MeshSavePath = string.Empty;

        GenerateSavePath();
    }

    void GenerateSavePath()
    {
        if (MaterialSavePath == string.Empty && MeshSavePath == string.Empty)
        {
            MaterialSavePath = $"Assets/level data/Resources/materials/{transform.name} {GenerateRandomString(5)}.mat";
            MeshSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(5)}.mesh";

#if UNITY_EDITOR
            while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(MaterialSavePath)))
            {
                MaterialSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(5)}.mat";
            }

            while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(MeshSavePath)))
            {
                MeshSavePath = $"Assets/level data/Resources/meshes/{transform.name} {GenerateRandomString(5)}.mesh";
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
                GenerateShape();
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
                        GenerateShape();

                        break;
                    }
                }
            }
        }
#endif
    }

    public class PolygonMeshGenerator
    {
        public static Mesh CreateMeshFromPolygon(List<Vector2> polygonPoints)
        {
            if (polygonPoints == null || polygonPoints.Count < 3)
            {
                Debug.LogError("Polygon must have at least 3 points.");
                return null;
            }

            // Create a new mesh
            Mesh mesh = new Mesh();

            // Convert 2D points to 3D vertices
            Vector3[] vertices = new Vector3[polygonPoints.Count];
            for (int i = 0; i < polygonPoints.Count; i++)
            {
                vertices[i] = new Vector3(polygonPoints[i].x, 0, polygonPoints[i].y);
            }

            // Create a triangulator to generate triangle indices
            Triangulator triangulator = new Triangulator(polygonPoints);
            int[] triangles = triangulator.Triangulate();

            // Assign vertices and triangles to the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // Recalculate normals and bounds for the mesh
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }

    public class Triangulator
    {
        private List<Vector2> points;

        public Triangulator(List<Vector2> points)
        {
            this.points = points;
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++) V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++) V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u) u = 0;
                v = u + 1;
                if (nv <= v) v = 0;
                int w = v + 1;
                if (nv <= w) w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = points[p];
                Vector2 qval = points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return A * 0.5f;
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = points[V[u]];
            Vector2 B = points[V[v]];
            Vector2 C = points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w)) continue;
                Vector2 P = points[V[p]];
                if (InsideTriangle(A, B, C, P)) return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}