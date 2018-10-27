using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackLocatable : MonoBehaviour {

    private float t = 0;

    public float parameter
    {
        get
        {
            return t;
        }
        set
        {
            t = value;
        }
    }
}
