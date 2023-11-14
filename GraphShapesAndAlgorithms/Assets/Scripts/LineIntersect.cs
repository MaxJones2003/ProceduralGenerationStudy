using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineIntersect : MonoBehaviour
{
    public Vector3 a, b, c, d;
    public Vector2f FindIntersection(Vector2f outside, Vector2f inside, Vector2f boundA, Vector2f boundB)
    {
        float a1 = inside.y - outside.y;
        float b1 = outside.x - inside.x;
        float c1 = (a1 * outside.x) + (b1 * outside.y);

        float a2 = boundB.y - boundA.y;
        float b2 = boundA.x - boundB.x;
        float c2 = (a2 * boundA.x) + (b2 * boundA.y);

        float det = a1 * b2 - a2 * b1;
        if(det == 0)
        {
            return new Vector2f(float.NaN, float.NaN);
        }
        else
        {
            float x = (b2 * c1 - b1 * c2) / det;
            float y = (a1 * c2 - a2 * c1) / det;

            // Check if the intersection point is within the bounds of both line segments
            if (x >= Math.Min(outside.x, inside.x) && x <= Math.Max(outside.x, inside.x) &&
                y >= Math.Min(outside.y, inside.y) && y <= Math.Max(outside.y, inside.y) &&
                x >= Math.Min(boundA.x, boundB.x) && x <= Math.Max(boundA.x, boundB.x) &&
                y >= Math.Min(boundA.y, boundB.y) && y <= Math.Max(boundA.y, boundB.y))
            {
                return new Vector2f(x, y);
            }
            else
            {
                return new Vector2f(float.NaN, float.NaN);
            }
        }
    }

    public Vector2f ConvertV3ToV2f(Vector3 p)
    {
        return new Vector2f(p.x, p.z);
    }
    public Vector3 ConvertV2fToV3(Vector2f p)
    {
        return new Vector3(p.x, 0, p.y);
    }

    void OnDrawGizmos()
    {
        // draw a point for for a b blue cd red ab line green cd line yellow (if intersection is not NaN black)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(a, 0.1f);
        Gizmos.DrawSphere(b, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(c, 0.1f);
        Gizmos.DrawSphere(d, 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(a, b);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(c, d);

        Vector2f intersection = FindIntersection(ConvertV3ToV2f(a), ConvertV3ToV2f(b), ConvertV3ToV2f(c), ConvertV3ToV2f(d));
        if (!float.IsNaN(intersection.x) && !float.IsNaN(intersection.y))
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(ConvertV2fToV3(intersection), 0.1f);
        }

    }
}
