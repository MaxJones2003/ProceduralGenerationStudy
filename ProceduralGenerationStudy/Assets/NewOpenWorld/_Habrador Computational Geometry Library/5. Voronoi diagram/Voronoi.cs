using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    //
    // 2d space
    //
    public class VoronoiEdge2
    {
        //These are the voronoi vertices
        public MyVector2 p1;
        public MyVector2 p2;

        //All positions within a voronoi cell is closer to this position than any other position in the diagram
        //Is also a vertex in the delaunay triangulation
        public MyVector2 sitePos;

        public VoronoiEdge2(MyVector2 p1, MyVector2 p2, MyVector2 sitePos)
        {
            this.p1 = p1;
            this.p2 = p2;

            this.sitePos = sitePos;
        }

        public override string ToString()
        {
            return $"p1: {p1} p2: {p2}";
        }

        public bool Equals(VoronoiEdge2 e)
        {
            if(p1.Equals(e.p1) && p2.Equals(e.p2)) return true;

            return false;
        }
    }


    public class VoronoiCell2
    {
        //All positions within a voronoi cell is closer to this position than any other position in the diagram
        //Is also a vertex in the delaunay triangulation
        public MyVector2 sitePos;

        public List<VoronoiEdge2> edges = new List<VoronoiEdge2>();

        public VoronoiCell2(MyVector2 sitePos)
        {
            this.sitePos = sitePos;
        }

        private float _area = -1;
        public float Area
        {
            get 
            {
                if(_area > -1) return _area;

                if(edges.Count < 3)
                {
                    _area = -MathUtility.EPSILON;
                    return _area;
                }

                if(edges.Count == 3)
                {
                    _area = CalculateTriangleArea(edges[0].p1, edges[1].p1, edges[2].p1);
                    return _area;
                }
                _area = 0;

                VoronoiEdge2 firstLine = edges[0];
                VoronoiEdge2 connectedLine1 = edges.Find(edge => edge.p2.Equals(firstLine.p2));
                VoronoiEdge2 connectedLine2 = edges.Find(edge => edge.p1.Equals(firstLine.p1));



                _area += CalculateTriangleArea(firstLine.p1, connectedLine1.p1, connectedLine2.p1);

                foreach(VoronoiEdge2 newLine in edges)
                {
                    if(newLine.Equals(firstLine) || newLine.Equals(connectedLine1) || newLine.Equals(connectedLine2)) continue;
                    _area += CalculateTriangleArea(firstLine.p1, newLine.p1, newLine.p2);

                }


                if(_area > 0) return _area;

                return -1;
            }
        }
        public float SignedPolygonArea()
        {
            // Add the first point to the end.
            int num_points = edges.Count;
            MyVector2[] Points = new MyVector2[num_points];
            for(int i = 0; i < num_points; i++) Points[i] = edges[i].p1;


            MyVector2[] pts = new MyVector2[num_points + 1];
            Points.CopyTo(pts, 0);
            pts[num_points] = Points[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area +=
                    (pts[i + 1].x - pts[i].x) *
                    (pts[i + 1].y + pts[i].y) / 2;
            }

            // Return the result.
            return Mathf.Abs(area);
        }

        private float CalculateTriangleArea(MyVector2 a, MyVector2 b, MyVector2 c)
        {
            // Calculate two vectors from the three vertices
            MyVector2 side1 = b - a;
            MyVector2 side2 = c - a;

            // Calculate the cross product of the two vectors
            float crossProduct = CrossProduct(side1, side2);

            // The magnitude of the cross product is the area of the triangle
            float area = 0.5f * Mathf.Abs(crossProduct);

            return area;
        }
        private float CrossProduct(MyVector2 a, MyVector2 b)
        {
            return a.x * b.x - a.y * b.x;
        }
        public void OrganizeEdges()
        {
            //foreach(var edge in edges) Debug.Log(edge);
            edges = edges.OrderBy(edge => edges.Any(e => edge.p1.Equals(e.p2) && !e.Equals(edge))).ThenBy(edge => edges.Any(e => !edge.p2.Equals(e.p1))).ToList();
            //foreach(var edge in edges) Debug.Log(edge);
            /* List<VoronoiEdge2> newEdges = edges;
            int i = 0;
            foreach(var edge1 in edges)
            {
                foreach(var edge2 in edges)
                {
                    if(edge1.p1.Equals(edge2.p2) && !edge1.Equals(edge2))
                    {
                        newEdges[i] = edge1;
                        i++;
                        continue;
                    }
                }
            }
            edges = newEdges; */
        }

        public override string ToString()
        {
            return $"VoronoiCell2 Site Position: {sitePos}. Edge Count: {edges.Count}";
        }
    }


    //
    // 3d space
    //
    public class VoronoiEdge3
    {
        //These are the voronoi vertices
        public MyVector3 p1;
        public MyVector3 p2;

        //All positions within a voronoi cell is closer to this position than any other position in the diagram
        //Is also a vertex in the delaunay triangulation
        public MyVector3 sitePos;

        public VoronoiEdge3(MyVector3 p1, MyVector3 p2, MyVector3 sitePos)
        {
            this.p1 = p1;
            this.p2 = p2;

            this.sitePos = sitePos;
        }
    }


    public class VoronoiCell3
    {
        //All positions within a voronoi cell is closer to this position than any other position in the diagram
        //Is also a vertex in the delaunay triangulation
        public MyVector3 sitePos;

        public List<VoronoiEdge3> edges = new List<VoronoiEdge3>();

        public VoronoiCell3(MyVector3 sitePos)
        {
            this.sitePos = sitePos;
        }
    }
}
