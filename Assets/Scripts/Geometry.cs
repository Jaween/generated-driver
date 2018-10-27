using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometry : MonoBehaviour {

    // Based on https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
    public static bool RayTriangleIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 p0, Vector3 p1, Vector3 p2, out  Vector3 intersection)
    {
        const float epsilon = 0.0000001f;
        intersection = new Vector3();

        var edge1 = p1 - p0;
        var edge2 = p2 - p0;

        var h = Vector3.Cross(rayDirection, edge2);
        var a = Vector3.Dot(edge1, h);

        // Ray parallel to triangle
        if (a > -epsilon && a < epsilon)
        {
            return false;
        }

        var f = 1.0f / a;
        var s = rayOrigin - p0;
        var u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f)
        {
            return false;
        }

        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(rayDirection, q);

        if (v < 0.0f || u + v > 1.0f)
        {
            return false;
        }

        var t = f * Vector2.Dot(edge2, q);
        if (t > epsilon)
        {
            intersection = rayOrigin + t * rayDirection;
            return true;
        }
        return false;
    }
}
