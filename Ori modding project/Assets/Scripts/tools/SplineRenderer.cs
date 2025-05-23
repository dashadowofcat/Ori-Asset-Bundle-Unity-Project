﻿using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class SplineRenderer : MonoBehaviour
{
    public EdgeCollider2D Edge;
    private Vector2[] _prevEdgePoints;

    [Header("Spline")]
    public SplineType CurveType;

    public int SplineCount = 25;
    public float SplineWidth = 0.1f;
    public float SplineOffset = 0f;

    public enum SplineType
    {
        Cubic,
        Bezier,
        CatmullRom
    }

    public Color Color = Color.black;

    public Material Material;

    public string MaterialSavePath;
    public string MeshSavePath;

    public bool autoSave = true;

    [Button("Generate Renderer")]
    public void GenerateShape()
    {
        if (transform.Find("Renderer") != null) DestroyImmediate(transform.Find("Renderer").gameObject);


        Vector2[] Points = Edge.points;

        for (int x = 0; x < Points.Length; x++)
        {
            Points[x] = transform.position + (Vector3)Points[x];
        }


        List<Vector2> InterpolatedPoints = new List<Vector2>();

        switch (CurveType)
        {
            case SplineType.Cubic:
                InterpolatedPoints = Cubic.Interpolate(Points, SplineCount).ToList();
                break;

            case SplineType.Bezier:
                InterpolatedPoints = Bezier.Interpolate(Points, SplineCount).ToList();
                break;

            case SplineType.CatmullRom:
                InterpolatedPoints = CatmullRom.Interpolate(Points, SplineCount).ToList();
                break;
        }

        Mesh SplineMesh = SplineMeshGenerator.CreateMeshFromSpline(InterpolatedPoints, SplineWidth, SplineOffset);

        GameObject Renderer = new GameObject("Renderer");

        Renderer.transform.parent = gameObject.transform;

        MeshFilter meshFilter = Renderer.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = Renderer.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material((Material)Resources.Load("Basic/White"));
        meshRenderer.sharedMaterial.color = Color;

        meshFilter.mesh = SplineMesh;

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
        if (autoSave && Tools.current != Tool.None && Edge != null)
        {
            if (_prevEdgePoints == null)
            {
                _prevEdgePoints = new Vector2[Edge.points.Count()];
                for (int i = 0; i < Edge.points.Count(); ++i)
                    _prevEdgePoints[i] = Edge.points[i];
            }
            else if (Edge.points.Count() != _prevEdgePoints.Count())
            {
                _prevEdgePoints = new Vector2[Edge.points.Count()];
                for (int i = 0; i < Edge.points.Count(); ++i)
                    _prevEdgePoints[i] = Edge.points[i];
                Debug.Log(gameObject.name + " point count changed");
                GenerateShape();
            }
            else
            {
                for (int i = 0; i < _prevEdgePoints.Count(); ++i)
                {
                    if (_prevEdgePoints[i] != Edge.points[i])
                    {
                        for (int j = 0; j < Edge.points.Count(); ++j)
                            _prevEdgePoints[j] = Edge.points[j];
                        Debug.Log(gameObject.name + " points changed");
                        GenerateShape();

                        break;
                    }
                }
            }
        }
#endif
    }

    public class SplineMeshGenerator
    {
        public static Mesh CreateMeshFromSpline(List<Vector2> splinePoints, float splineWidth, float offset = 0f)
        {
            if (splinePoints == null || splinePoints.Count < 2)
            {
                Debug.LogError("Spline must have at least 2 points.");
                return null;
            }

            // Create a new mesh
            Mesh mesh = new Mesh();

            // Convert 2D points to 3D vertices
            Vector3[] vertices = new Vector3[splinePoints.Count * 2];
            int[] triangles = new int[2 * (splinePoints.Count - 1) * 3];
            int vertexIndex = 0;
            int triangleIndex = 0;

            for(int i = 0; i < splinePoints.Count; ++i)
            {
                Vector2 forward = Vector2.zero;
                if(i < splinePoints.Count - 1)
                {
                    forward += splinePoints[i + 1] - splinePoints[i];
                }

                if(i > 0)
                {
                    forward += splinePoints[i] - splinePoints[i - 1];
                }

                forward.Normalize();

                Vector2 left = new Vector2(-forward.y, forward.x);

                vertices[vertexIndex] = splinePoints[i] - left * offset;
                vertices[vertexIndex + 1] = splinePoints[i] - left * (splineWidth + offset);

                if(i < splinePoints.Count - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 2;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = vertexIndex + 2;
                    triangles[triangleIndex + 5] = vertexIndex + 3;
                }

                vertexIndex += 2;
                triangleIndex += 6;
            }

            // Assign vertices and triangles to the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // Recalculate normals and bounds for the mesh
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }

    public static class Cubic
    {
        public static Vector2[] Interpolate(Vector2[] points, int count)
        {
            if (points == null || points.Length < 2)
                throw new ArgumentException("points must contain at least 2 points");

            int inputPointCount = points.Length;
            double[] inputDistances = new double[inputPointCount];
            for (int i = 1; i < inputPointCount; i++)
            {
                double distance = Vector2.Distance(points[i], points[i - 1]);
                inputDistances[i] = inputDistances[i - 1] + distance;
            }

            double meanDistance = inputDistances.Last() / (count - 1);
            double[] evenDistances = Enumerable.Range(0, count).Select(x => x * meanDistance).ToArray();
            double[] xs = points.Select(p => (double)p.x).ToArray();
            double[] ys = points.Select(p => (double)p.y).ToArray();
            double[] xsOut = Interpolate(inputDistances, xs, evenDistances);
            double[] ysOut = Interpolate(inputDistances, ys, evenDistances);

            return xsOut.Zip(ysOut, (x, y) => new Vector2((float)x, (float)y)).ToArray();
        }

        private static double[] Interpolate(double[] xOrig, double[] yOrig, double[] xInterp)
        {
            (double[] a, double[] b) = FitMatrix(xOrig, yOrig);

            double[] yInterp = new double[xInterp.Length];
            for (int i = 0; i < yInterp.Length; i++)
            {
                int j;
                for (j = 0; j < xOrig.Length - 2; j++)
                    if (xInterp[i] <= xOrig[j + 1])
                        break;

                double dx = xOrig[j + 1] - xOrig[j];
                double t = (xInterp[i] - xOrig[j]) / dx;
                double y = (1 - t) * yOrig[j] + t * yOrig[j + 1] +
                    t * (1 - t) * (a[j] * (1 - t) + b[j] * t);
                yInterp[i] = y;
            }

            return yInterp;
        }

        private static (double[] a, double[] b) FitMatrix(double[] x, double[] y)
        {
            int n = x.Length;
            double[] a = new double[n - 1];
            double[] b = new double[n - 1];
            double[] r = new double[n];
            double[] A = new double[n];
            double[] B = new double[n];
            double[] C = new double[n];

            double dx1, dx2, dy1, dy2;

            dx1 = x[1] - x[0];
            C[0] = 1.0f / dx1;
            B[0] = 2.0f * C[0];
            r[0] = 3 * (y[1] - y[0]) / (dx1 * dx1);

            for (int i = 1; i < n - 1; i++)
            {
                dx1 = x[i] - x[i - 1];
                dx2 = x[i + 1] - x[i];
                A[i] = 1.0f / dx1;
                C[i] = 1.0f / dx2;
                B[i] = 2.0f * (A[i] + C[i]);
                dy1 = y[i] - y[i - 1];
                dy2 = y[i + 1] - y[i];
                r[i] = 3 * (dy1 / (dx1 * dx1) + dy2 / (dx2 * dx2));
            }

            dx1 = x[n - 1] - x[n - 2];
            dy1 = y[n - 1] - y[n - 2];
            A[n - 1] = 1.0f / dx1;
            B[n - 1] = 2.0f * A[n - 1];
            r[n - 1] = 3 * (dy1 / (dx1 * dx1));

            double[] cPrime = new double[n];
            cPrime[0] = C[0] / B[0];
            for (int i = 1; i < n; i++)
                cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);

            double[] dPrime = new double[n];
            dPrime[0] = r[0] / B[0];
            for (int i = 1; i < n; i++)
                dPrime[i] = (r[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);

            double[] k = new double[n];
            k[n - 1] = dPrime[n - 1];
            for (int i = n - 2; i >= 0; i--)
                k[i] = dPrime[i] - cPrime[i] * k[i + 1];

            for (int i = 1; i < n; i++)
            {
                dx1 = x[i] - x[i - 1];
                dy1 = y[i] - y[i - 1];
                a[i - 1] = k[i - 1] * dx1 - dy1;
                b[i - 1] = -k[i] * dx1 + dy1;
            }

            return (a, b);
        }
    }

    public static class Bezier
    {
        public static Vector2[] Interpolate(Vector2[] controlPoints, int numPoints)
        {
            if (controlPoints == null || controlPoints.Length < 2)
                throw new ArgumentException("controlPoints must contain at least 2 points");

            List<Vector2> result = new List<Vector2>();

            for (int i = 0; i <= numPoints; i++)
            {
                float t = (float)i / numPoints;
                result.Add(CalculateBezierPoint(t, controlPoints));
            }

            return result.ToArray();
        }

        private static Vector2 CalculateBezierPoint(float t, Vector2[] controlPoints)
        {
            int n = controlPoints.Length - 1;
            Vector2 point = new Vector2(0, 0);

            for (int i = 0; i <= n; i++)
            {
                float binomialCoefficient = BinomialCoefficient(n, i);
                float term = binomialCoefficient * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
                point += term * controlPoints[i];
            }

            return point;
        }

        private static int BinomialCoefficient(int n, int k)
        {
            int result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= n - (k - i);
                result /= i;
            }
            return result;
        }
    }

    public static class CatmullRom
    {
        public static Vector2[] Interpolate(Vector2[] points, int numPoints)
        {
            if (points == null || points.Length < 4)
                throw new ArgumentException("points must contain at least 4 points");

            List<Vector2> result = new List<Vector2>();

            for (int i = 0; i < points.Length - 3; i++)
            {
                for (int j = 0; j <= numPoints; j++)
                {
                    float t = (float)j / numPoints;
                    result.Add(CalculateCatmullRomPoint(t, points[i], points[i + 1], points[i + 2], points[i + 3]));
                }
            }

            return result.ToArray();
        }

        private static Vector2 CalculateCatmullRomPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float a0 = -0.5f * t3 + t2 - 0.5f * t;
            float a1 = 1.5f * t3 - 2.5f * t2 + 1.0f;
            float a2 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
            float a3 = 0.5f * t3 - 0.5f * t2;

            return new Vector2(
                a0 * p0.x + a1 * p1.x + a2 * p2.x + a3 * p3.x,
                a0 * p0.y + a1 * p1.y + a2 * p2.y + a3 * p3.y
            );
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