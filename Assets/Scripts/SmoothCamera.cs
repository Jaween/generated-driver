using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour {

    public BezierSpline spline;
    public Vehicle3 vehicle;
    public float anticipationDistance;
    public float pitch;
    public float followDistance;
    public float followDamping = 0.4f;

    private Vector3 velocity = Vector3.zero;

	void FixedUpdate() {
        // Look ahead of the vehicle (anticipate the curve) by some amount
        var anticipationParameter = anticipationDistance / spline.CurveLength;
        var parameter = vehicle.GetComponent<TrackLocatable>().parameter + anticipationParameter;
        var lookTarget = spline.GetEvenPoint(parameter);

        var splineForward = spline.GetEvenDirection(parameter);
        var splineUp = Vector3.up;
        var splineLeft = Vector3.Cross(splineForward, splineUp).normalized;

        var pitchRotation = Quaternion.AngleAxis(-pitch, splineLeft);
        var targetPosition = lookTarget - pitchRotation * splineForward * followDistance;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followDamping);

        var lookForward = (lookTarget - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookForward, Vector3.up);
	}
}
