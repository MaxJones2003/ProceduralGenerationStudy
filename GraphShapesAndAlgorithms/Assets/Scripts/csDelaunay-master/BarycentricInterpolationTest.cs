using System.Collections;
using System.Collections.Generic;
using Map;
using UnityEngine;

public class BarycentricInterpolationTest : MonoBehaviour
{
    public Vector2f point;
    float elevation = 0;
    public List<Vector3> points;
    Center center;

    void OnDrawGizmos()
    {
        if(center == null)
        {
            center = new Center();
            center.corners = new List<Corner>();
        }
        foreach (var corner in center.corners)
        {
            Gizmos.DrawSphere(new Vector3(corner.point.x, corner.elevation, corner.point.y), 0.1f);
        }
        Gizmos.DrawSphere(new Vector3(point.x, elevation, point.y), 0.1f);
    }

    void OnValidate()
    {
        center.corners = new List<Corner>();

        foreach(Vector3 point in points)
        {
            Corner corner = new Corner
            {
                point = new Vector2f(point.x, point.z),
                elevation = point.y
            };
            center.corners.Add(corner);
        }
        
        elevation = ElevationCalculator.GetElevation(center, point);
    }
}
