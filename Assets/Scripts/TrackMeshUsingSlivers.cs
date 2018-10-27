using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackMeshUsingSlivers : MonoBehaviour {

    public BezierSpline spline;
    public GameObject trackSliver;


    public float runningT = 0;
    public float runningLength = 0.1f;
    private GameObject[] generated = new GameObject[40];

    private void Start()
    {
        GenerateTrack(runningT, runningLength, generated, spline);
    }

    void Update () {
        for (int i = 0; i < generated.Length; i++)
        {
            GameObject.Destroy(generated[i]);
        }

        runningT = (runningT + 0.006f) % 1.0f;
        GenerateTrack(runningT, runningLength, generated, spline);
    }

    private void GenerateTrack(float startT, float length, GameObject[] container, BezierSpline spline)
    {
        float deltaT = container.Length / length;
        for (int i = 0; i < container.Length; i++)
        {
            float t = (startT + length * i / (float)container.Length) % 1.0f;
            Vector3 position = spline.GetPoint(t);
            container[i] = GameObject.Instantiate(trackSliver, position, Quaternion.identity);
            container[i].transform.LookAt(position + spline.GetDirection(t));
        }
    }
}
