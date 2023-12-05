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
    // a inspector button that calls a center.OrderCornersClockwise() method

    public void OrderCorners()
    {
        center.OrderCornersClockwise();
        points.Clear();
        foreach(Corner corner in center.corners)
        {
            points.Add(new Vector3(corner.point.x, corner.elevation, corner.point.y));
        }
    }

    void OnDrawGizmos()
    {
        if(center == null)
        {
            center = new Center();
            center.corners = new List<Corner>();
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < center.corners.Count; i++)
        {
            Corner corner = center.corners[i];
            Gizmos.color = ColorAtIndex(i);
            Gizmos.DrawSphere(new Vector3(corner.point.x, corner.elevation, corner.point.y), 0.1f);
        }
        if(BarycentricInterpolationTest.IsPointInPolygon(center.corners, point))
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(point.x, elevation, point.y), 0.1f);

        // draw lines between corners
        Gizmos.color = Color.black;
        for (int i = 0; i < center.corners.Count; i++)
        {
            Corner corner = center.corners[i];
            Gizmos.color = ColorAtIndex(i);
            Gizmos.DrawLine(new Vector3(corner.point.x, corner.elevation, corner.point.y), new Vector3(center.corners[(i + 1) % center.corners.Count].point.x, center.corners[(i + 1) % center.corners.Count].elevation, center.corners[(i + 1) % center.corners.Count].point.y));
        }
    }
    Color ColorAtIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.green;
            case 2:
                return Color.blue;
            case 3:
                return Color.yellow;
            case 4:
                return Color.magenta;
            case 5: 
                return Color.cyan;
            case 6:
                return Color.gray;
            default:
                return Color.black;
        }
        return Color.black;
    }

    void OnValidate()
    {
        center.corners = new List<Corner>();
        IDWInterpolator iDWInterpolator = new IDWInterpolator(center.corners, 2, 100);

        foreach(Vector3 point in points)
        {
            Corner corner = new Corner
            {
                point = new Vector2f(point.x, point.z),
                elevation = point.y
            };
            center.corners.Add(corner);
        }
        
        elevation = IsPointInPolygon(center.corners, point) ? iDWInterpolator.InverseDistanceWeighting(point) : 0;
    }

    public static bool IsPointInPolygon(List<Corner> polygon, Vector2f testPoint)
    {
        /* bool result = false;
        int j = polygon.Count - 1;
        for (int i = 0; i < polygon.Count; i++)
        {
            if(i == j) continue;
            if (polygon[i].point.y < testPoint.y && polygon[j].point.y >= testPoint.y || 
                polygon[j].point.y < testPoint.y && polygon[i].point.y >= testPoint.y)
            {
                if (polygon[i].point.x + (testPoint.y - polygon[i].point.y) /
                   (polygon[j].point.y - polygon[i].point.y) *
                   (polygon[j].point.x - polygon[i].point.x) < testPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result; */

        List<Vector2f> points = new List<Vector2f>();
        polygon.ForEach(c => points.Add(c.point));
        return InsidePolygonHelper.checkInside(points, points.Count, testPoint) == 1;
    }
}
