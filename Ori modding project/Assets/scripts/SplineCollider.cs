﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SplineCollider : MonoBehaviour
{
    public EdgeCollider2D Edge;

    [Header("Spline")]
    public SplineType CurveType;

    public int SplineCount = 100;

    public Color GizmosCurveColor;

    public Color GizmosPointColor;

    public float GizmosPointSize;

    [Header("Collision Settings")]
    public float ColliderWidth;

    public enum SplineType
    {
        Cubic,
        Bezier,
        CatmullRom
    }

    [EditorCools.Button]
    void GenerateCollision()
    {
        if(transform.Find("Collision") != null) DestroyImmediate(transform.Find("Collision").gameObject);

        Vector2[] Points = Edge.points;

        for (int x = 0; x < Points.Length; x++)
        {
            Points[x] = transform.position + (Vector3)Points[x];
        }


        List<Vector2> InterpolatedPoints = new List<Vector2>();

        switch(CurveType)
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

        GameObject ColliderGameObject = new GameObject("Collision");

        ColliderGameObject.transform.parent = transform;

        for (int y = 0; y < InterpolatedPoints.ToArray().Length; y++)
        {
            if (y == InterpolatedPoints.ToArray().Length - 1) break;

            InstanciateCollisionPlane(InterpolatedPoints[y], InterpolatedPoints[y + 1], ColliderGameObject.transform, ColliderWidth);
        }
    }

    [EditorCools.Button]
    void RemoveCollision()
    {
        if (transform.Find("Collision") != null) DestroyImmediate(transform.Find("Collision").gameObject);
    }

    public void InstanciateCollisionPlane(Vector2 pointA, Vector2 pointB, Transform Parent, float QuadWidth)
    {
        Vector2 midpoint = (pointA + pointB) / 2f;

        float distance = Vector2.Distance(pointA, pointB);

        float angle = Mathf.Atan2(pointB.y - pointA.y, pointB.x - pointA.x) * Mathf.Rad2Deg;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

        quad.transform.position = new Vector3(midpoint.x, midpoint.y, 0);

        quad.transform.rotation = Quaternion.Euler(90 + angle, -90, 90);

        quad.transform.localScale = new Vector3(distance, 1, 1);

        quad.transform.localScale = new Vector3(quad.transform.localScale.x, QuadWidth, quad.transform.localScale.z);

        DestroyImmediate(quad.GetComponent<MeshRenderer>());

        quad.transform.parent = Parent;
    }


    void Reset()
    {
        Edge = GetComponent<EdgeCollider2D>();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = GizmosCurveColor;

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

        for (int y = 0; y < InterpolatedPoints.ToArray().Length; y++)
        {
            if (y == InterpolatedPoints.ToArray().Length - 1) break;
        
            Gizmos.DrawLine((Vector3)InterpolatedPoints[y], (Vector3)InterpolatedPoints[y + 1]);
        }

        Gizmos.color = GizmosPointColor;

        foreach (Vector2 point in InterpolatedPoints)
        {
            Gizmos.DrawSphere((Vector3)point, GizmosPointSize);
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
}