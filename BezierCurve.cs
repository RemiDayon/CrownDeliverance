using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
    List<Vector3> points = null;
    
    public void SetPoints(List<Vector3> _points)
    {
        points = _points;
    }

    // _t has to be between 0 and 1
    public Vector3 GetPosition(float _t)
    {
        Vector3 result = new Vector3(0f, 0f, 0f);
        for (int x = 0; x < points.Count; ++x)
        {
            result += points[x] * Mathf.Pow(_t, x) * Mathf.Pow(1 - _t, points.Count - 1 - x);
        }
        return result;
    }
   
}
