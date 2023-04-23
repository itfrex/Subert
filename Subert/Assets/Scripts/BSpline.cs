using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSpline
{
    private Matrix4x4 m;

    private List<Vector2> points;

    private Vector4 t;

    public BSpline(Vector2 initialPos)
    {
        m = new Matrix4x4(new Vector4(1, 4, 1, 0), new Vector4(-3, 0, 3, 0), new Vector4(3, -6, 3, 0), new Vector4(-1, 3, -3, 1));
        points = new List<Vector2>();
        points.Add(initialPos);
    }
    public Vector2 Evaluate(float value)
    {
        value = Mathf.Clamp01(value);
        value *= points.Count;
        if (value < points.Count -1)
        {
            Vector4[] pointArrays = ConstructPointArrays((int)value);
            value = value == points.Count ? value % 1 + 1 : value % 1;
            t = new Vector4(1, value, Mathf.Pow(value, 2), Mathf.Pow(value, 3));
            Vector4 newVec = m * t / 6;
            return new Vector2(Vector4.Dot(newVec, pointArrays[0]), Vector4.Dot(newVec, pointArrays[1]));
        }
        else
        {
            return points[points.Count - 1];
        }

    }
    private Vector4[] ConstructPointArrays(int index)
    {
        Vector4[] result = new Vector4[2];
        Vector2 v1;
        Vector2 v2;
        Vector2 v3;
        Vector2 v4;
        v1 = index == 0 ? (2 * points[0] - points[1]) : points[index-1];
        v2 = points[index];
        v3 = points[index+1];
        v4 = index+2 >= points.Count ? (2 * points[index+1] - points[index]) : points[index+2];

        result[0] = new Vector4(v1.x, v2.x, v3.x, v4.x);
        result[1] = new Vector4(v1.y, v2.y, v3.y, v4.y);
        return result;
    }
    public void AddPoint(Vector2 p)
    {
        points.Add(p);
    }
    private void RemoveOldestPoint()
    {
        points.RemoveAt(0);
    }
    public void RemoveToEvalPoint(float evalPoint)
    {
        Vector2 pos = Evaluate(evalPoint);
        for(int i = (int)(Mathf.Clamp01(evalPoint) * points.Count); i > 0; i--)
        {
            if(points.Count > 1)
            {
                RemoveOldestPoint();
            }
            
        }
        points[0] = pos;

    }
}
