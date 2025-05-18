using UnityEngine;

[RequireComponent(typeof(PathCreator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PathMeshCreator : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 1;
    public float pathWidth = 1;
    public bool autoUpdate = false;

    public void UpdatePath()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoints(spacing, 1);
        GetComponent<MeshFilter>().mesh = CreatePathMesh(points);
    }

    Mesh CreatePathMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if ( i < points.Length - 1) // Not the last point
            {
                forward += points[i + 1] - points[i];
            }

            if(i > 0) // Not the first point
            {
                forward += points[i] - points[i - 1];
            }

            forward.Normalize();
            Vector3 top = new Vector3(-forward.y, forward.x, 0);
            top.Normalize();

            verts[vertIndex] = points[i];
            verts[vertIndex + 1] = points[i] - new Vector3(top.x, top.y, points[i].z) * pathWidth;

            if(i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 2;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;

        return mesh;
    }
}
