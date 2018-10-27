using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackMesh))]
public class BezierMesh : Editor {

    private TrackMesh trackMesh;
    
    void OnSceneGUI()
    {
        if (trackMesh.autoUpdate && Event.current.type == EventType.Repaint)
        {
            trackMesh.UpdateMesh();
        }
    }

    private void OnEnable()
    {
        trackMesh = (TrackMesh)target;
    }
}
