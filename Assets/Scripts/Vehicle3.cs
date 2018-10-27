using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TrackLocatable))]
public class Vehicle3 : MonoBehaviour {

    public BezierSpline spline;
    public TrackMesh trackMesh;
    public Transform vehicleMesh;

    public float elevation;
    public float latAcc;
    public float latFric;
    public float latMaxVel;
    public float speed;

    private float xAcc = 0;
    private float xVel = 0;
    private float x = 0;

    private TrackLocatable trackLocatable;

    private void Start()
    {
        trackLocatable = GetComponent<TrackLocatable>();
    }

    private void Update()
    {
        HandleLateralInput();
        HandleForwardInput();
    }

    private void FixedUpdate()
    {

        var points = spline.EvenlySpacedPoints;
        var progressRatio = trackLocatable.parameter * (points.Length - 1);
        var forward = spline.GetEvenDirection(trackLocatable.parameter);
        var worldUp = new Vector3(0.0f, 1.0f, 0.0f);
        var left = Vector3.Cross(forward, worldUp).normalized;
        transform.up = Vector3.Cross(left, forward).normalized;
        transform.forward = forward;

        var elevationOffset = Vector3.up * elevation;
        var lateralOffset = transform.right * x;
        transform.position = spline.GetEvenPoint(trackLocatable.parameter) + elevationOffset + lateralOffset;


        Debug.DrawLine(transform.position, transform.position + spline.GetEvenDirection(trackLocatable.parameter), Color.magenta);
    }

    private void HandleLateralInput()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.position.x < Screen.width / 2)
            {
                xAcc = -latAcc;
            }
            else
            {
                xAcc = latAcc;
            }
        }
        else
        {

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                xAcc = -latAcc;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                xAcc = latAcc;
            }
            else
            {
                xAcc = 0;
            }
        }

        var vehicleWidth = 1.0f;
        xVel += xAcc * Time.fixedDeltaTime;
        xVel *= latFric;
        xVel = Mathf.Clamp(xVel, -latMaxVel, latMaxVel);
        x += xVel * Time.fixedDeltaTime;
        x = Mathf.Clamp(x, -trackMesh.width / 2 + vehicleWidth / 4, trackMesh.width / 2 - vehicleWidth / 4);
        vehicleMesh.localRotation = Quaternion.Euler(0, 0, -xAcc);
    }

    private void HandleForwardInput()
    {
        var vertical = 0.3f;// Input.GetAxis("Vertical");
        trackLocatable.parameter += vertical * speed / spline.CurveLength * Time.fixedDeltaTime;
        trackLocatable.parameter = Mathf.Clamp01(trackLocatable.parameter);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }

    /*IEnumerator AccTilt()
    {
        while ()
        yield return new WaitForEndOfFrame();
    }*/
}
