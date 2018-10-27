using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour {

    public TrackLocatable vehicle;
    public BezierSpline spline;
    public TrackMesh mesh;
    public float randomSpread;
    public float length;

    public GameObject[] arr;
    private Vector3 runningDirection = Vector3.forward;

    public void Generate()
    {
        AddNewTrack(length, randomSpread);
    }

    private void AddNewTrack(float length, float randomSpread)
    {
        Vector3 generatedAnchor;
        Vector3 generatedControl1;
        Vector3 generatedControl2;
        GenerateSegment(out generatedAnchor, out generatedControl1, out generatedControl2);

        // Adds new segment onto the track
        var previousTrackLength = spline.CurveLength;
        spline.AddCurve(generatedControl1, generatedControl2, generatedAnchor);
        spline.SetControlPointMode(spline.ControlPointCount - 1, BezierControlPointMode.Mirrored);

        mesh.UpdateMesh();
    }
    
    void GenerateSegment(out Vector3 anchor, out Vector3 control1, out Vector3 control2)
    {
        var currentEnd = spline.GetControlPoint(spline.ControlPointCount - 1);
        var forward = spline.GetDirection(1);
        var up = new Vector3(0.0f, 1.0f, 0.0f);
        var left = Vector3.Cross(forward, up).normalized;

        var randomisedForward = (forward + left * Random.Range(-randomSpread / 2, randomSpread / 2)).normalized;
        runningDirection += (randomisedForward * length).normalized;

        var extra =  forward * Random.Range(-randomSpread / 2, randomSpread / 2);
        int random = Random.Range(0, 2);
        if (random == 0)
        {
            // Right
            anchor = currentEnd + forward * length - left * length / 2 + extra;
            control2 = currentEnd + forward * length + left * length * 0.1f;
            anchor += Vector3.up * 0.1f;
            control2 -= Vector3.up * 0.1f;
        } else if (random == 1)
        {
            // Left
            anchor = currentEnd + forward * length + left * length / 2 + extra;
            control2 = currentEnd + forward * length - left * length * 0.1f;
            anchor += Vector3.up * 0.1f;
            control2 -= Vector3.up * 0.1f;
        } else
        {
            // Forward
            anchor = currentEnd + forward * length * 0.75f + extra;
            control2 = anchor - forward * length * 0.05f;
        }

        control1 = Vector3.zero;
    }
}
