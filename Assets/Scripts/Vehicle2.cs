using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle2 : MonoBehaviour {

    public BezierSpline spline;
    public float elevation;

    private float t = 0;
    private float angle = 0;
    private float x = 0;
    private float upPressed = 0;

    void FixedUpdate()
    {
        var points = spline.EvenlySpacedPoints;
        var progressRatio = t * (points.Length - 1);
        var forward = spline.GetDirection(t).normalized;
        var worldUp = new Vector3(0.0f, 1.0f, 0.0f);
        var left = Vector3.Cross(forward,worldUp).normalized;
        transform.up = Vector3.Cross(left, forward).normalized;

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var upIncrease = vertical * Time.fixedDeltaTime * 0.1f;

        angle += horizontal * Time.fixedDeltaTime * 100;
        x += Mathf.Sin(Mathf.Deg2Rad * angle) * upIncrease * 20;
        t += Mathf.Cos(Mathf.Deg2Rad * angle) * upIncrease;

        x = Mathf.Clamp(x, -1, 1);
        
        transform.forward = Quaternion.AngleAxis(angle, transform.up) * forward;

        t = Mathf.Clamp(t, 0, 1);
        int index = (int)(t * (float)points.Length - 1);
        Debug.DrawLine(transform.position, transform.position + forward * 2);
        Debug.DrawLine(transform.position, transform.position + transform.up, Color.magenta);

        var elevationOffset = transform.up * elevation;
        transform.position = points[index];// - left * x + elevationOffset;
    }
}
