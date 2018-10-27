using UnityEngine;
using System;
using System.Collections.Generic;

public class BezierSpline : MonoBehaviour {

	[SerializeField]
	private Vector3[] points;
    public float[] segmentLengths;
    private Vector3[] evenlySpacedPoints;
    private float curveLength;

	[SerializeField]
	public BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

    private void Awake()
    {
        segmentLengths = new float[CurveCount];
        for (var i = 0; i < CurveCount; i++)
        {
            var length = Bezier.Length(points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3]);
            segmentLengths[i] = length;
            curveLength += length;
        }
    }

    public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
                // TODO: Segment lengths
			}
		}
	}

    public Vector3[] GetPointsInSegment(int index)
    {
        return new Vector3[]
        {
           points[index * 3],
           points[index * 3 + 1],
           points[index * 3 + 2],
           points[index * 3 + 3],
        };
    }

	public int ControlPointCount {
		get {
			return points.Length;
		}
	}

    public float CurveLength
    {
        get
        {
            return curveLength;
        }
    }

	public Vector3 GetControlPoint (int index) {
		return points[index];
	}

    public Vector3[] EvenlySpacedPoints
    {
        get
        {
            if (evenlySpacedPoints == null)
            {
                evenlySpacedPoints = ComputeEvenlySpacedPoints();
            }
            return evenlySpacedPoints;
        }
    }

	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
			if (loop) {
				if (index == 0) {
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1) {
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else {
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
                // Segment lengths
			}
			else {
				if (index > 0) {
					points[index - 1] += delta;
                    var length = Bezier.Length(points[index - 3], points[index - 2], points[index - 1], point);
                    segmentLengths[(index - 1) / 3] = length;
                    curveLength += length;
				}
				if (index + 1 < points.Length) {
					points[index + 1] += delta;
                    var length = Bezier.Length(point, points[index + 1], points[index + 2], points[index + 3]);
                    segmentLengths[index / 3] = length;
                    curveLength += length;
                }

			}
		}
		points[index] = point;
		EnforceMode(index);
        evenlySpacedPoints = ComputeEvenlySpacedPoints();
	}

	public BezierControlPointMode GetControlPointMode (int index) {
        try
        {
            return modes[(index + 1) / 3];
        } catch (IndexOutOfRangeException)
        {
            return modes[0];
        }
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
        try
        {
            modes[modeIndex] = mode;
        }catch (IndexOutOfRangeException)
        {
            return;
        }
		if (loop) {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) {
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
        BezierControlPointMode mode;
        try
        {
            mode = modes[modeIndex];
        }
        catch (IndexOutOfRangeException)
        {
            return;
        }
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
        // TODO Segment lengths
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 3;
		}
	}

    [System.Obsolete("Doesn't provide even spacing")]
	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {  
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}

    public Vector3 GetEvenPoint(float oldT)
    {
        int i;
        float newT;
        GetEvenCurveT(oldT, out i, out newT);

        return transform.TransformDirection(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], newT));
    }

    public Vector3 GetEvenVelocity(float oldT)
    {
        int i;
        float t;
        GetEvenCurveT(oldT, out i, out t);
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
    }

    public Vector3 GetEvenDirection(float oldT)
    {
        return GetEvenVelocity(oldT).normalized;
    }

    private void GetEvenCurveT(float t, out int index, out float newT) {
        var curveIndex = 0;
        var targetDistance = t * curveLength;
        var runningDistance = 0.0f;
        for (var i = 0; i < CurveCount; i++)
        {
            if (targetDistance < runningDistance + segmentLengths[i])
            {
                curveIndex = i;
                break;
            }
            runningDistance += segmentLengths[i];
        }

        index = curveIndex * 3;
        newT = (targetDistance - runningDistance) / segmentLengths[curveIndex];
    }

    [System.Obsolete("Doesn't provide even spacing")]
    public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}

    [System.Obsolete("Doesn't provide even spacing")]
    public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	public void AddCurve () {
        Vector3 point0 = points[points.Length - 1];
        Vector3 point1 = points[points.Length - 1];
        Vector3 point2 = points[points.Length - 1];
        point0.x += 1f;
        point1.x += 2f;
        point2.x += 3f;
        AddCurve(point0, point1, point2);
    }

    public void AddCurve(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Array.Resize(ref points, points.Length + 3);
        points[points.Length - 3] = p0;
        points[points.Length - 2] = p1;
        points[points.Length - 1] = p2;

        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        Array.Resize(ref segmentLengths, segmentLengths.Length + 1);
        var length = Bezier.Length(points[points.Length - 4], points[points.Length - 3], points[points.Length - 2], points[points.Length - 1]);
        segmentLengths[segmentLengths.Length - 1] = length;
        curveLength += length;

        if (loop)
        {
            points[points.Length - 1] = points[0];
            modes[modes.Length - 1] = modes[0];
            EnforceMode(0);
        }
        evenlySpacedPoints = ComputeEvenlySpacedPoints();
    }

    private void DeleteFirstCurve()
    {
        var tempPoints = new Vector3[points.Length - 3];
        Array.Copy(points, 3, tempPoints, 0, tempPoints.Length);
        points = tempPoints;

        var tempModes = new BezierControlPointMode[modes.Length - 1];
        Array.Copy(modes, 1, tempModes, 0, tempModes.Length);
        modes = tempModes;
        
        var oldLength = segmentLengths[0];
        var tempSegmentLengths = new float[segmentLengths.Length - 1];
        Array.Copy(segmentLengths, 1, tempSegmentLengths, 0, tempSegmentLengths.Length);
        segmentLengths = tempSegmentLengths;
        curveLength -= oldLength;
    }

    public void Shorten(float targetLength)
    {
        while (curveLength > targetLength)
        {
            DeleteFirstCurve();
        }
        evenlySpacedPoints = ComputeEvenlySpacedPoints();
    }
    
    private Vector3[] ComputeEvenlySpacedPoints(float spacing = 0.2f)
    {
        spacing = Mathf.Max(spacing, 0.05f);

        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float distanceSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < points.Length / 3; segmentIndex++)
        {
            Vector3[] p = GetPointsInSegment(segmentIndex);

            // Crude estimation of Bezier segment length
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * 10);

            for (var t = 1.0f / divisions; t <= 1.0f; t += 1.0f / divisions) {
                Vector3 pointOnCurve = Bezier.GetPoint(p[0], p[1], p[2], p[3], t);
                distanceSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (distanceSinceLastEvenPoint >= spacing)
                {
                    float overshootDistance = distanceSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDistance;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    distanceSinceLastEvenPoint = overshootDistance;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    public void Reset() {
        points = new Vector3[] {
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f),
            new Vector3(4f, 0f, 0f)
        };
        modes = new BezierControlPointMode[] {
            BezierControlPointMode.Mirrored,
            BezierControlPointMode.Mirrored
        };
        segmentLengths = new float[] { (points[3] - points[0]).magnitude };
        curveLength = segmentLengths[0];
        evenlySpacedPoints = ComputeEvenlySpacedPoints();
	}
}