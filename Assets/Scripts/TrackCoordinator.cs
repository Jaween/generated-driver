using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BezierSpline))]
[RequireComponent(typeof(TrackGenerator))]
public class TrackCoordinator : MonoBehaviour {

    public Vehicle3 vehicle;
    public float maxTrackLength;
    public TrackLocatable[] spawnables;

    private BezierSpline track;
    private TrackGenerator trackGenerator;

    private List<TrackLocatable> spawned = new List<TrackLocatable>();

    private void Start()
    {
        track = GetComponent<BezierSpline>();
        trackGenerator = GetComponent<TrackGenerator>();
    }

    private void FixedUpdate()
    {
        var vehicleLocatable = vehicle.GetComponent<TrackLocatable>();

        // Generating new track and shortening old track
        if (vehicleLocatable.parameter > 0.6f || track.CurveLength < maxTrackLength)
        {
            var vehicleProgress = vehicleLocatable.parameter * track.CurveLength;
            trackGenerator.Generate();
            vehicleLocatable.parameter = vehicleProgress / track.CurveLength;
            GenerateSpawnable();
        }

        // Saving details
        var previousVehicleProgress = vehicleLocatable.parameter * track.CurveLength;
        var previousTrackLength = track.CurveLength;
        var spawnableOldPositions = new float[spawned.Count];
        for (var i = 0; i < spawnableOldPositions.Length; i++)
        {
            spawnableOldPositions[i] = spawned[i].parameter * previousTrackLength;
        }

        // Shortening
        if (vehicleLocatable.parameter > 0.5f && previousVehicleProgress > (track.segmentLengths[0] + track.segmentLengths[1]))
        {
            track.Shorten(track.CurveLength - track.segmentLengths[0]);
        }

        // Reposition track items
        Reposition(vehicleLocatable, previousVehicleProgress, previousTrackLength, track.CurveLength);
        for (var i = 0; i < spawnableOldPositions.Length; i++)
        {
            Reposition(spawned[i], spawnableOldPositions[i], previousTrackLength, track.CurveLength);
        }
    }

    void Reposition(TrackLocatable trackLocatable, float previousProgress, float previousTrackLength, float trackLength)
    {
        var removedLength = previousTrackLength - trackLength;
        trackLocatable.parameter = Mathf.Clamp01((previousProgress - removedLength) / trackLength);
    }

    void GenerateSpawnable()
    {
        var spawnable = spawnables[Random.Range(0, spawnables.Length - 1)];
        var forward = track.GetDirection(1);
        var up = new Vector3(0.0f, 1.0f, 0.0f);
        var left = Vector3.Cross(forward, up);
        var position = track.GetEvenPoint(1) + left * Random.Range(-trackGenerator.mesh.width / 2, trackGenerator.mesh.width / 2);
        spawned.Add(GameObject.Instantiate(spawnable, position, Quaternion.identity));
    }
}
