using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrackGenerator : MonoBehaviour {

    public TrackLocatable vehicle;
    public BezierSpline spline;
    public TrackMesh mesh;
    public int anchorGenerationCount;
    public float hDegrees;
    public float length;
    
    private Vector3[] splinePoints = null;
    private Vector3 forward = Vector3.right;

    public void Generate()
    {
        var previousAnchor = spline.GetControlPoint(spline.ControlPointCount - 1);
        var previousControl = spline.GetControlPoint(spline.ControlPointCount - 2);

        // Random random anchors with control points automatically smoothed
        var anchors = GenerateAnchors(anchorGenerationCount, previousAnchor, ref forward);
        splinePoints = SmoothBezierFromAnchors(anchors);

        for (var i = 1; i < splinePoints.Length; i += 3) {
            spline.AddCurve(splinePoints[i], splinePoints[i + 1], splinePoints[i + 2]);
            spline.SetControlPointMode(spline.ControlPointCount - 1, BezierControlPointMode.Mirrored);
        }
        mesh.UpdateMesh();
    }

    Vector3[] GenerateAnchors(int anchorCount, Vector3 startAnchor, ref Vector3 forward)
    {
        var horizontalAngle = Random.Range(-hDegrees / 2, hDegrees / 2);

        var anchors = new Vector3[anchorCount];
        var previousAnchor = startAnchor;
        for (var i = 0; i < anchorCount; i++)
        {
            forward = Quaternion.Euler(new Vector3(0, horizontalAngle, 0)) * forward;
            forward.Normalize();
            anchors[i] = previousAnchor + forward * length;
            previousAnchor = anchors[i] + Vector3.up * 0.1f;
        }
        return anchors;
    }
    
    // Sets control points around the anchors such that the resulting spline is smooth
    Vector3[] SmoothBezierFromAnchors(Vector3[] anchors)
    {
        float[] controlPointXs = new float[anchors.Length];
        for (var i = 0; i < anchors.Length; i++)
        {
            controlPointXs[i] = anchors[i].x;
        }
        float[] controlPointYs = new float[anchors.Length];
        for (var i = 0; i < anchors.Length; i++)
        {
            controlPointYs[i] = anchors[i].y;
        }
        float[] controlPointZs = new float[anchors.Length];
        for (var i = 0; i < anchors.Length; i++)
        {
            controlPointZs[i] = anchors[i].z;
        }

        float[] p1X, p1Y, p1Z;
        float[] p2X, p2Y, p2Z;
        ComputeControlPoints(controlPointXs, out p1X, out p2X);
        ComputeControlPoints(controlPointYs, out p1Y, out p2Y);
        ComputeControlPoints(controlPointZs, out p1Z, out p2Z);

        // Combining contorl points and anchors
        var anchorsAndControls = new Vector3[anchors.Length * 3 - 2];
        var index = 0;
        for (var i = 0; i < anchors.Length - 1; i++)
        {
            anchorsAndControls[index++] = anchors[i];
            anchorsAndControls[index++] = new Vector3(p1X[i], p1Y[i], p1Z[i]);
            anchorsAndControls[index++] = new Vector3(p2X[i], p2Y[i], p2Z[i]);
        }
        anchorsAndControls[index++] = anchors[anchors.Length - 1];
        return anchorsAndControls;
    }

    // Converted to C# from information and JavaScript code provided here:
    // https://www.particleincell.com/2012/bezier-splines/
    void ComputeControlPoints(float[] k, out float[] p1, out float[] p2)
    {
        p1 = new float[k.Length];
        p2 = new float[k.Length];
        var n = k.Length - 1;

        var a = new float[k.Length];
        var b = new float[k.Length];
        var c = new float[k.Length];
        var r = new float[k.Length];

        // First anchor
        a[0] = 0;
        b[0] = 2;
        c[0] = 1;
        r[0] = k[0] + 2 * k[1];

        // Internal anchors
        for (var i = 1; i < k.Length - 1; i++)
        {
            a[i] = 1;
            b[i] = 4;
            c[i] = 1;
            r[i] = 4 * k[i] + 2 * k[i + 1];
        }

        // Last anchor
        a[n - 1] = 2;
        b[n - 1] = 7;
        c[n - 1] = 0;
        r[n - 1] = 8 * k[n - 1] + k[n];

        // Solve Ax = b with Thomas algorithm
        for (var i = 1; i < n; i++)
        {
            var m = a[i] / b[i - 1];
            b[i] = b[i] - m * c[i - 1];
            r[i] = r[i] - m * r[i - 1];
        }

        // Solve for p1
        p1[n - 1] = r[n - 1] / b[n - 1];
        for (var i = n - 2; i >= 0; i--)
        {
            p1[i] = (r[i] - c[i] * p1[i + 1]) / b[i];
        }

        // Solve for p2
        for (var i = 0; i < n - 1; i++)
        {
            p2[i] = 2 * k[i + 1] - p1[i + 1];
        }
        p2[n - 1] = (k[n] + p1[n - 1]) / 2;
    }

    private void OnDrawGizmos()
    {
        DrawSpline(splinePoints, Color.magenta);
        DrawAnchorsAndControls(splinePoints, 0.1f);
    }

    // Helper to show draw points along the spline
    void DrawAnchorsAndControls(Vector3[] spline, float size)
    {
        for (var i = 0; i < spline.Length; i++)
        {
            Gizmos.color = i % 3 == 0 ? Color.yellow : Color.blue;
            Gizmos.DrawSphere(spline[i], size);
        }
    }

    // Helper to show the spline itself
    void DrawSpline(Vector3[] points, Color color)
    {
        for (var i = 0; i < points.Length - 3; i += 3)
        {
            var previousPoint = points[i];
            if (i + 3 != points.Length)
            {
                int divisions = 20;
                for (var t = 0.0f; t <= 1.0f; t += 1.0f / divisions)
                {
                    var point = Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
                    Debug.DrawLine(previousPoint + Vector3.up, point + Vector3.up, color);
                    previousPoint = point;
                }
                Debug.DrawLine(previousPoint + Vector3.up, points[i + 3] + Vector3.up, color);
            }
        }
    }
}
