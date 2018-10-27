using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour {

    public BezierSpline spline;
    public TrackMesh trackMesh;
    public MeshFilter meshFilter;
    public float speed;
    public float elevation;

    private float angle = 0;
    private Vector3 planeIntersection = Vector3.zero;
    private Vector3 normal = Vector3.up;

    private void FixedUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        angle += horizontal * 270 * Time.fixedDeltaTime;
        transform.rotation = Quaternion.AngleAxis(angle, normal);

        transform.position += transform.forward * vertical * Time.fixedDeltaTime;

        UpdateUpDirection();
        
    }

    private void UpdateUpDirection()
    {
        var mesh = meshFilter.mesh;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            // Indices and vertices making up this triangle
            var i0 = mesh.triangles[i];
            var i1 = mesh.triangles[i + 1];
            var i2 = mesh.triangles[i + 2];
            var t0 = mesh.vertices[i0];
            var t1 = mesh.vertices[i1];
            var t2 = mesh.vertices[i2];

            var n = ComputeNormal(t0, t1, t2);
            var avg = (t0 + t1 + t2) / 3;
            Debug.DrawLine(avg, avg + n, Color.magenta);

            Vector3 intersection;
            if (Geometry.RayTriangleIntersection(transform.position, -transform.up, t0, t1, t2, out intersection))
            {
                planeIntersection = intersection;

                normal = ComputeNormal(t0, t1, t2);
                transform.position = intersection + normal * elevation;
                break;
            }
        }
    }

    private Vector3 ComputeNormal(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Vector3.Cross(p1 - p0, p2 - p0).normalized;
    }

}
