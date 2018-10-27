using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BezierSpline))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TrackMesh : MonoBehaviour {

    public float width = 1;
    public bool autoUpdate = false;

    public void UpdateMesh()
    {
        var spline = GetComponent<BezierSpline>();
        var points = spline.EvenlySpacedPoints;
        GetComponent<MeshFilter>().mesh = CreateMesh(points);
    }

    Mesh CreateMesh(Vector3[] points)
    {
        var vertices = new Vector3[points.Length * 2];
        var uvs = new Vector2[vertices.Length];
        var triangles = new int[2 * (points.Length - 1) * 3];

        var vertexIndex = 0;
        var triangleIndex = 0;

        // Creating vertices and triangles along the curve line of the track
        for (var i = 0; i < points.Length; i++)
        {
            // Averaging the directions to the next and previous point, to compute the forward direction
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1)
            {
                forward += points[i + 1] - points[i];
            }
            if (i > 0)
            {
                forward += points[i] - points[i - 1]; 
            }
            forward.Normalize();

            // Compute the left direction using a predefined up direction
            var up = new Vector3(0.0f, 1.0f, 0.0f);
            var left = Vector3.Cross(forward, up);

            // Vertices on either side of this point
            vertices[vertexIndex] = points[i] + left * width/2;
            vertices[vertexIndex + 1] = points[i] - left * width/2;

            // Texture coordinates
            var completionRatio = i / (float)(points.Length - 1);
            uvs[vertexIndex] = new Vector2(0, completionRatio);
            uvs[vertexIndex + 1] = new Vector2(1, completionRatio);

            // Defining two triangles at this point
            if (i < points.Length - 1)
            {
                triangles[triangleIndex++] = vertexIndex;
                triangles[triangleIndex++] = vertexIndex + 2;
                triangles[triangleIndex++] = vertexIndex + 1;

                triangles[triangleIndex++] = vertexIndex + 1;
                triangles[triangleIndex++] = vertexIndex + 2;
                triangles[triangleIndex++] = vertexIndex + 3;
            }

            vertexIndex += 2;
        }

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        return mesh;
    }
}
