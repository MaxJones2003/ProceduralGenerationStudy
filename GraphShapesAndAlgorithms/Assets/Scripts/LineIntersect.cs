using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Map;

public class Poly
{
    public static float epsilon = 0.00001f;
    public Vector2f[] Q;
    public Poly(Vector2f[] Q)
    {
        this.Q = Q;
    }
    public int Inside(Vector2f P)
    {
        int result = 0;
        int j = Q.Length - 1;
        for (int i = 0; i < Q.Length; i++)
        {
            if (Q[i].y < P.y && Q[j].y >= P.y || 
                Q[j].y < P.y && Q[i].y >= P.y)
            {
                if (Q[i].x + (P.y - Q[i].y) /
                   (Q[j].y - Q[i].y) *
                   (Q[j].x - Q[i].x) < P.x)
                {
                    result++;
                }
            }
            j = i;
        }
        return result;
    }

    public bool InsideBool(Vector2f P)
    {
        int result = 0;
        int j = Q.Length - 1;
        for (int i = 0; i < Q.Length; i++)
        {
            if (Q[i].y < P.y && Q[j].y >= P.y ||
                Q[j].y < P.y && Q[i].y >= P.y)
            {
                if (Q[i].x + (P.y - Q[i].y) /
                   (Q[j].y - Q[i].y) *
                   (Q[j].x - Q[i].x) < P.x)
                {
                    result++;
                }
            }
            j = i;
        }
        return result % 2 != 0;
    }

    float Max(float a, float b)
    {
        return a > b ? a : b;
    }
    float Min(float a, float b)
    {
        return a < b ? a : b;
    }
}
public class LineIntersect : MonoBehaviour
{
    private Vector3[] polygon;
    private Vector2f[] polyPoints;
    private Vector2f query;

    public void SetUp(List<Map.Corner> corners)
    {
        return;
        polyPoints = GetBorders(corners);
        //for (int i = 0; i < 1; i++) OrderCornersClockwise();
        //Order();
        
        poly = new Poly(polyPoints);
        Debug.Log(poly.Q.Length);
    }
    private Vector2f[] GetBorders(List<Corner> corners)
    {
        List<Vector2f> borders = new List<Vector2f>();
        List<Corner> cornerBorders = new List<Corner>();
        foreach (Corner c in corners)
        {
            if (c.coast)
            {
                cornerBorders.Add(c);
                int x = 0;
                foreach(var q in c.adjacent)
                    if(q.coast) x++;
                break;
            }
            
        }

        // recursively look through the found border points
        // if it is a border, add it to the list, 
        // then search the newest point
        // make sure a found point isnt already contained in the borders list
        bool foundFirstPoint = false;
        int borderIndex = 0;
        int attempt = 0;
        Corner currentCorner = cornerBorders[borderIndex];
        Corner previousCorner = cornerBorders[borderIndex];
        while(!foundFirstPoint)
        {
            currentCorner = cornerBorders[borderIndex];
            if(NumOfAdjacentCoasts(currentCorner) == 3)
            {
                // this is a special case and needs to be handled differently
                // check the num of adjacent coasts of each adjacent corner if it is a coast
                // if its a coast and has 3 adj coasts, ignore it, if we dont, we get infinite loops kinda
                //int indexWithThreeCoasts = -1;
                //int nextIndex = -1;
                //bool foundNext = false;
                

                // this doesnt work :(
                //foreach(var q in cornerBorders[borderIndex].touches)
                    foreach(Corner c in cornerBorders[borderIndex].touches[0].corners)    
                    {
                        if(c.coast && c.index != previousCorner.index && c.adjacent.Contains(previousCorner))
                        {
                            cornerBorders.Add(c);
                            borderIndex++;
                            break;
                        }
                    }   

            }
            else
            {
                foreach (Corner c in cornerBorders[borderIndex].adjacent) 
                {
                    if (c.coast)
                    {
                        //Debug.Log("coast found");
                        if(c.index == cornerBorders[0].index && borderIndex > 3)
                        {
                            foundFirstPoint = true;
                            //Debug.Log("first found at index "+ borderIndex);
                            break;
                        }
                        if(!cornerBorders.Contains(c))
                        {
                            //Debug.Log("added point \n" + c.index);
                            cornerBorders.Add(c);
                            borderIndex++;
                            break;
                        }
                    }
                }
            }
            if(attempt > 100000)
            {
                Debug.Log(NumOfAdjacentCoasts(currentCorner));
                Debug.Log("hit limit");
                break;
            } 
            attempt++;
            previousCorner = currentCorner;
        }

        return cornerBorders.Select(c => c.point).ToArray();
    }

    private int NumOfAdjacentCoasts(Corner corner)
    {
        int adj = 0;
        for(int i = 0; i < corner.adjacent.Count; i++)
        {
            adj += corner.adjacent[i].coast ? 1 : 0;
        }
        return adj;
    }

    public bool IsInside(Vector2f point)
    {
        return false;
        return poly.InsideBool(point);
    }
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
    private Poly poly;
    void OnValidate()
    {
        //OrderCornersClockwise();
        //polyPoints = new Vector2f[polygon.Length];
        //for(int i = 0; i < polygon.Length; i++)
        //{
        //    polyPoints[i] = ConvertV3ToV2f(polygon[i]);
        //}
        //poly = new Poly(polyPoints);
    }
    public int drawLines = 1;

    void OnDrawGizmos()
    {
        if(poly == null) return;

        for(int i = 0; i < poly.Q.Length; i++)
        {
            Vector3 a = new Vector3(poly.Q[i].x, 0, poly.Q[i].y);
            Gizmos.DrawSphere(a, 2f);
            if(i < drawLines)
            {
                Vector3 b = new Vector3(poly.Q[(i + 1) % poly.Q.Length].x, 0, poly.Q[(i + 1) % poly.Q.Length].y);
                Gizmos.DrawLine(a, b);
            }
        }
        return;
        for(int i = 0; i < poly.Q.Length; i++)
        {
            Vector3 a = new Vector3(poly.Q[i].x, 0, poly.Q[i].y);
            Vector3 b = new Vector3(poly.Q[(i+1)%poly.Q.Length].x, 0, poly.Q[(i+1)%poly.Q.Length].y);
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(a, 0.1f);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(a, b);
        }
        int x = poly.Inside(query);
        Gizmos.color = x % 2 != 0 ? Color.green : Color.red;
        Gizmos.DrawSphere(new Vector3(query.x, 0, query.y), 0.01f);
        Gizmos.DrawLine(new Vector3(query.x, 0, query.y), new Vector3(query.x, 0, query.y) - Vector3.right*1000);
        Handles.Label(new Vector3(query.x, 0, query.y), x.ToString());

    }

    public void OrderCornersClockwise()
    {
        if (polygon.Length == 0)
            return;
        List<Vector2f> points = new List<Vector2f>();
        points = polygon.Select(p => ConvertV3ToV2f(p)).ToList();

        // set the reference point to the left most corner
        Vector2f referencePoint = points.OrderBy(c => c.y).ThenBy(c => c.x).First();

        points.Sort((a, b) =>
        {
            float angleA = Mathf.Atan2(a.y - referencePoint.y, a.x - referencePoint.x);
            float angleB = Mathf.Atan2(b.y - referencePoint.y, b.x - referencePoint.x);

            // Adjust the angles to be between 0 and 2π
            angleA = (angleA + 2 * Mathf.PI) % (2 * Mathf.PI);
            angleB = (angleB + 2 * Mathf.PI) % (2 * Mathf.PI);

            // Reverse the comparison to sort in clockwise order
            return angleB.CompareTo(angleA);
        });
        for(int i = 0; i < points.Count; i++)
        {
            polygon[i] = ConvertV2fToV3(points[i]);
        }
    }

    public void Order()
    {
        OrderCornersClockwise();
       /* polyPoints = new Vector2f[polygon.Length];
        for(int i = 0; i < polygon.Length; i++)
        {
            polyPoints[i] = ConvertV3ToV2f(polygon[i]);
        }
        poly = new Poly(polyPoints);*/
    }
}

/* if(NumOfAdjacentCoasts(currentCorner.adjacent[j]) == 2 && currentCorner.adjacent[j].coast)
                    {
                        // this is the one we want
                        if(currentCorner.adjacent[j].index == cornerBorders[0].index && borderIndex > 3)
                        {
                            foundFirstPoint = true;
                            //Debug.Log("special first found at index "+ borderIndex);
                            break;
                        }
                        if(!cornerBorders.Contains(currentCorner.adjacent[j]))
                        {
                            //Debug.Log("special added point \n" + currentCorner.adjacent[j].index);
                            cornerBorders.Add(currentCorner.adjacent[j]);
                            borderIndex++;
                            foundNext = true;
                            nextIndex = j;
                            if(indexWithThreeCoasts == -1) continue;
                            else break;
                        }
                    }
                    else if(NumOfAdjacentCoasts(currentCorner.adjacent[j]) == 3 && currentCorner.adjacent[j].coast)
                    {
                        indexWithThreeCoasts = j;
                    }
                    if(foundNext && indexWithThreeCoasts != -1)
                    {
                        // we found the next coast, and have the index of the next next coast
                        // add it???
                        if(!cornerBorders.Contains(currentCorner.adjacent[indexWithThreeCoasts]) && currentCorner.adjacent[nextIndex].adjacent.Contains(currentCorner.adjacent[indexWithThreeCoasts]))
                        {
                            //Debug.Log("special added point \n" + currentCorner.adjacent[j].index);
                            cornerBorders.Add(currentCorner.adjacent[indexWithThreeCoasts]);
                            borderIndex++;
                            break;
                        }
                    } */
        /* THIS DIDN'T WORK
        
        // P - Point in question
        // Q - Polygon in question
        // R - Ray, starting at point P going right to infinity
        Vector2f R = P;
        R.x = float.MaxValue;
        // A - Vertex of edge, y value is less than B
        Vector2f A;
        // B - Vertex of edge, y value is greater than A
        Vector2f B;

        bool inside = false;
        for(int i = 0; i < Q.Length; i++)
        {
            A = Q[i].y < Q[(i+1)%Q.Length].y ? Q[i] : Q[(i+1)%Q.Length];
            B = Q[i].y > Q[(i+1)%Q.Length].y ? Q[i] : Q[(i+1)%Q.Length];
            

            if(P.y == A.y || P.y == B.y) P.y += epsilon; // Make sure the point is not at the same height as a vertex

            if(P.y > B.y || P.y < A.y || P.x > Max(A.x, B.x))
            {
                // The ray doesn't intersect the edge
                continue;
            }

            if(P.x < Min(A.x, B.x))
            {
                inside = !inside;
                continue;
            }

            float edge = (B.y - A.y) / (B.x - A.x);
            float point = (B.y - A.y) / (P.x - A.x);

            if(point >= edge)
            {
                inside = !inside;
                continue;
            }
        }
        return inside; */
