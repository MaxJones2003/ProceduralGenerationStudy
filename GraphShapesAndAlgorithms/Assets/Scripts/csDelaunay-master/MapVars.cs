using System.Collections.Generic;
using csDelaunay;
using System.Linq;
using System;
using System.Drawing;
using System.Net.Sockets;
using System.Numerics;
using Unity.VisualScripting;

namespace Map
{
    public class Map
    {
        int SIZE;
        public List<Center> centers;
        public List<Corner> corners;
        public List<Edge> edges;

        public Map(int SIZE, Voronoi voronoi)
        {
            this.SIZE = SIZE;
            // Foreach site location in the sites indexed by location dictionary add points
            List<Vector2f> points = new();
            foreach (var v in voronoi.SitesIndexedByLocation.Keys)
                points.Add(v);

            BuildGraph(points, voronoi);
        }
        // https://github.com/amitp/mapgen2/blob/master/Map.as
        private void BuildGraph(List<Vector2f> points, Voronoi voronoi)
        {
             Corner q;// Vector2f point, other;
            var libEdges = voronoi.Edges;
            Dictionary<Vector2f, Center> centerLookup = new();

            // Build Center objects for each of the points, and a lookup map
            // to find those Center objects again as we build the graph
            foreach (var point in points)
            {
                Center p = new Center();
                p.index = centers.Count;
                p.point = point;
                p.neighbors = new List<Center>();
                p.borders = new List<Edge>();
                p.corners = new List<Corner>();
                centers.Add(p);
                centerLookup.Add(point, p);
            }

            // Workaround for Voronoi lib bug: we need to call region()
            // before Edges or neighboringSites are available
            // I don't know if I actually need this, the reference I am using uses a different voronoi library, so I might not have this bug, 
            // but I'm doing it just to be safe.
            foreach (var p in centers)
            {
                voronoi.Region(p.point);
            }
            // The Voronoi library generates multiple Point objects for
            // corners, and we need to canonicalize to one Corner object.
            // To make lookup fast, we keep an array of Points, bucketed by
            // x value, and then we only have to look at other Points in
            // nearby buckets. When we fail to find one, we'll create a new
            // Corner object.
            List<List<Corner>> _cornerMap = new();

            Corner makeCorner(Vector2f point) {
                Corner q;

                if (point == null) return null;
                int bucket = 0;

                for (bucket = (int) point.x - 1; bucket <= (int) point.x+1 + 1; bucket++) {
                    foreach(var _q in _cornerMap[bucket]) {
                        var dx = point.x - _q.point.x;
                        var dy = point.y - _q.point.y;
                        if (dx * dx + dy * dy < 1e-6)
                        {
                            return _q;
                        }
                    }
                }

                bucket = (int)point.x;
                if (_cornerMap[bucket] == null) _cornerMap[bucket] = new();
                q = new Corner();
                q.index = corners.Count;
                corners.Add(q);
                q.point = point;
                q.border = (point.x == 0 || point.x == SIZE
                            || point.y == 0 || point.y == SIZE);
                q.touches = new List<Center>();
                q.protrudes = new List<Edge>();
                q.adjacent = new List<Corner>();
                _cornerMap[bucket].Add(q);
                return q;
            }

            // Helper functions for the following loop; ideally thes would be inline
            void addToCorners(List<Corner> v, Corner x)
            {
                if(x != null && v.IndexOf(x) < 0) v.Add(x);
            }
            void addToCenterList(List<Center> v, Center x)
            {
                if(x != null && v.IndexOf(x) < 0) v.Add(x);
            }

            foreach(var libEdge in libEdges)
            {
                var dedge = libEdge.DelaunayLine();
                var vedge = libEdge.VoronoiEdge();

                // Fill the graph data. Make an Edge object corresponding to
                // the edge from the voronoi library.
                var edge = new Edge();
                edge.index = edges.Count;
                edge.river = 0;
                edges.Add(edge);

                // Edges point to corners. Edges point to centers.
                edge.v0 = makeCorner(vedge.p0);
                edge.v1 = makeCorner(vedge.p1);

            }
        }
            
        
    }
}

