using System.Collections.Generic;
using csDelaunay;
using System.Linq;
using System;
using System.Drawing;
using System.Net.Sockets;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace Map
{
        // https://github.com/amitp/mapgen2/blob/master/Map.as
    public class Map
    {
        static public float LAKE_THRESHOLD = 0.3f;  // 0 to 1, fraction of water corners for water polygon
        int SIZE;
        System.Random mapRandom = new System.Random();

        // Island shape is controlled by the islandRandom seed and the
        // type of island, passed in when we set the island shape. The
        // islandShape function uses both of them to determine whether any
        // point should be water or land.


        // Point selection is random for the original article, with Lloyd
        // Relaxation, but there are other ways of choosing points. Grids
        // in particular can be much simpler to start with, because you
        // don't need Voronoi at all. HOWEVER for ease of implementation,
        // I continue to use Voronoi here, to reuse the graph building
        // code. If you're using a grid, generate the graph directly.
        
        Func<Vector2f, bool> islandShape;
        public int numPoints;
        public int lloydIterations = 2;

        public List<Vector2f> points;
        public List<Center> centers;
        public List<Corner> corners;
        public List<Edge> edges;

        /// <summary>
        /// A grid of squares containing all centers located in each cell
        /// </summary>
        public Grid grid;

        public Map(int numOfPoints, int SIZE, int iterations, int maxStage)
        {
            numPoints = numOfPoints;
            this.SIZE = SIZE;
            lloydIterations = iterations;
            //numPoints = 1;
            goStage = maxStage;
            Reset();
        }
         // Random parameters governing the overall shape of the island
         int goStage = 0;
        public void  NewIsland(IslandShapeEnum islandShapeEnum, int numPoints_, int seed) 
        {
            numPoints = numPoints_;
            SetIslandShape(islandShapeEnum, seed);
            numPoints = numPoints_;
            mapRandom = new System.Random(seed);
            Go(0, goStage);
        }
        private void SetIslandShape(IslandShapeEnum islandShapeEnum, int seed)
        {
            switch (islandShapeEnum)
            {
                case IslandShapeEnum.Radial:
                    islandShape = IslandShape.MakeRadial(seed);
                    break;
                case IslandShapeEnum.Perlin:
                    islandShape = IslandShape.MakePerlin(seed);
                    break;
                case IslandShapeEnum.Square:
                    islandShape = IslandShape.MakeSquare(seed);
                    break;
                case IslandShapeEnum.Blob:
                    islandShape = IslandShape.MakeBlob(seed);
                    break;
                default:
                    throw new ArgumentException("Invalid island type: " + islandShapeEnum);
            }
        }

        public void Reset()
        {
            // Break cycles so the garbage collector will release data.
            if (centers != null)
            {
                foreach (var p in centers)
                {
                    p.neighbors.Clear();
                    p.corners.Clear();
                    p.borders.Clear();
                }
                centers.Clear();
            }

            if (corners != null)
            {
                foreach (var q in corners)
                {
                    q.adjacent.Clear();
                    q.touches.Clear();
                    q.protrudes.Clear();
                    q.downslope = null;
                    q.watershed = null;
                }
                corners.Clear();
            }

            if (edges != null)
            {
                foreach (var edge in edges)
                {
                    edge.d0 = null;
                    edge.d1 = null;
                    edge.v0 = null;
                    edge.v1 = null;
                }
                edges.Clear();
            }

            // Initialize the lists if they are null
            centers ??= new List<Center>();
            corners ??= new List<Corner>();
            edges ??= new List<Edge>();

            // In C#, the garbage collector runs automatically, so there's no need to call it manually.
        }

        public void Go(int first, int last)
        {
            var stages = new List<Tuple<string, Action>>();

            void TimeIt(string name, Action action)
            {
                var stopwatch = Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                UnityEngine.Debug.Log($"{name}: {stopwatch.ElapsedMilliseconds} ms");
            }

            // Generate the initial random set of points
            stages.Add(new Tuple<string, Action>("Place points...",
                () =>
                {
                    Reset();
                    points = CreateRandomPoints(numPoints);
                }));

            // Create a graph structure from the Voronoi edge list.
            stages.Add(new Tuple<string, Action>("Build graph...",
                () =>
                {
                    var voronoi = GenerateVoronoi(points, lloydIterations);
                    BuildGraph(points, voronoi);
                    voronoi.Dispose();
                    voronoi = null;
                    points = null;
                }));

            stages.Add(new Tuple<string, Action>("Assign elevations...",
                () =>
                {
                    // Determine the elevations and water at Voronoi corners.
                    AssignCornerElevations();

                    // Determine polygon and corner type: ocean, coast, land.
                    AssignOceanCoastAndLand();

                    // Rescale elevations so that the highest is 1.0.
                    // Assign elevations to non-land corners.
                    // Polygon elevations are the average of their corners.
                    RedistributeElevations(LandCorners(corners));

                    // Assign elevations to non-land corners
                    foreach (var q in corners)
                    {
                        if (q.ocean || q.coast)
                        {
                            q.elevation = 0.0f;
                        }
                    }

                    // Polygon elevations are the average of their corners
                    AssignPolygonElevations();
                }));

            stages.Add(new Tuple<string, Action>("Assign moisture...",
                () =>
                {
                    // Determine downslope paths.
                    CalculateDownslopes();

                    // Determine watersheds.
                    CalculateWatersheds();

                    // Create rivers.
                    CreateRivers();

                    // Determine moisture at corners, starting at rivers
                    // and lakes, but not oceans. Then redistribute
                    // moisture to cover the entire range evenly from 0.0
                    // to 1.0. Then assign polygon moisture as the average
                    // of the corner moisture.
                    AssignCornerMoisture();
                    RedistributeMoisture(LandCorners(corners));
                    AssignPolygonMoisture();
                }));

            stages.Add(new Tuple<string, Action>("Decorate map...",
                () =>
                {
                    AssignBiomes();
                }));

            for (int i = first; i < last; i++)
            {
                TimeIt(stages[i].Item1, stages[i].Item2);
            }
        }

        public List<Vector2f> CreateRandomPoints(int size) 
        {
            // Use Vector2f, instead of Vector2
            // Vector2f is pretty much the same as Vector2, but like you could run Voronoi in another thread
            List<Vector2f> points = new List<Vector2f>();
            for (int i = 0; i < size; i++) {
                int x = UnityEngine.Random.Range(0,SIZE);
                int y = UnityEngine.Random.Range(0,SIZE);
                points.Add(new Vector2f(x, y));
                if(points[i].x < 0 || points[i].x > SIZE || points[i].y < 0 || points[i].y > SIZE) UnityEngine.Debug.Log("Point outside of map: " + points[i].x + ", " + points[i].y);

            }
            return points;
        }
        public Voronoi GenerateVoronoi(List<Vector2f> points, int iterations)
        {
            return new Voronoi(points,new Rectf(0,0,SIZE,SIZE),iterations);
        }

        public List<Corner> LandCorners(List<Corner> corners) 
        {
            List<Corner> locations = new();
            foreach (var q in corners) {
                if (!q.ocean && !q.coast) {
                    locations.Add(q);
                }
                }
            return locations;
            }
        
        private void BuildGraph(List<Vector2f> points, Voronoi voronoi)
        {
            var libEdges = voronoi.Edges;

            Dictionary<Vector2f, Center> centerLookup = new();

            // Build Center objects for each of the points, and a lookup map
            // to find those Center objects again as we build the graph
            foreach (var point in voronoi.SitesIndexedByLocation.Keys)
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
            Vector2f InvalidVector2f = new Vector2f(float.NaN, float.NaN);

           Dictionary<Vector2f, Corner> _cornerMap = new Dictionary<Vector2f, Corner>();

            Corner makeCorner(Vector2f point)
            {
                if (float.IsNaN(point.x) || float.IsNaN(point.y)) 
                { 
                    UnityEngine.Debug.Log("Returning null"); 
                    return null; 
                }

                if (_cornerMap.TryGetValue(point, out Corner existingCorner))
                {
                    return existingCorner;
                }

                var newCorner = new Corner
                {
                    index = corners.Count,
                    point = point,
                    border = (point.x == 0 || point.x == SIZE || point.y == 0 || point.y == SIZE),
                    touches = new List<Center>(),
                    protrudes = new List<Edge>(),
                    adjacent = new List<Corner>()
                };

                corners.Add(newCorner);
                _cornerMap[point] = newCorner;

                return newCorner;
            }

            // Helper functions for the following loop; ideally thes would be inline
            void addToCornerList(List<Corner> v, Corner x)
            {
                if(x != null && v.IndexOf(x) < 0) v.Add(x);
            }
            void addToCenterList(List<Center> v, Center x)
            {
                if(x != null && v.IndexOf(x) < 0) v.Add(x);
            }
            float nan = float.NaN;
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
                edge.midpoint = (vedge.p0.x != nan && vedge.p1.y != nan) ? Vector2f.Interpolate(vedge.p0, vedge.p1, 0.5f) : InvalidVector2f ;

                // Edges point to corners. Edges point to centers.
                edge.v0 = makeCorner(vedge.p0);
                edge.v1 = makeCorner(vedge.p1);
                edge.d0 = centerLookup[dedge.p0];
                edge.d1 = centerLookup[dedge.p1];

                // Centers point to edges. Corners point to edges
                if (edge.d0 != null) { edge.d0.borders.Add(edge); }
                if (edge.d1 != null) { edge.d1.borders.Add(edge); }
                if (edge.v0 != null) { edge.v0.protrudes.Add(edge); }
                if (edge.v1 != null) { edge.v1.protrudes.Add(edge); }

                // Centers point to centers.
                if (edge.d0 != null && edge.d1 != null)
                {
                    addToCenterList(edge.d0.neighbors, edge.d1);
                    addToCenterList(edge.d1.neighbors, edge.d0);
                }

                // Corners point to corners
                if (edge.v0 != null && edge.v1 != null)
                {
                    addToCornerList(edge.v0.adjacent, edge.v1);
                    addToCornerList(edge.v1.adjacent, edge.v0);
                }

                // Centers point to corners
                if (edge.d0 != null)
                {
                    addToCornerList(edge.d0.corners, edge.v0);
                    addToCornerList(edge.d0.corners, edge.v1);
                }
                if (edge.d1 != null)
                {
                    addToCornerList(edge.d1.corners, edge.v0);
                    addToCornerList(edge.d1.corners, edge.v1);
                }

                // Corners point to centers
                if (edge.v0 != null)
                {
                    addToCenterList(edge.v0.touches, edge.d0);
                    addToCenterList(edge.v0.touches, edge.d1);
                }
                if (edge.v1 != null)
                {
                    addToCenterList(edge.v1.touches, edge.d0);
                    addToCenterList(edge.v1.touches, edge.d1);
                }
            }
            //ClipVoronoiEdges();
            // find and set all corners border value if it is on the border of the map using lamda expression

            foreach(Corner c in corners)
            {
                if(c.point == Vector2f.zero)
                {
                    var testCorner = c;
                    corners.Remove(testCorner);
                    corners.Remove(testCorner.adjacent[0]);
                    break;  
                }
            }

            //UnityEngine.Debug.Log(corner.protrudes.Count);
            //corners.ForEach(c => c.border = c.point.x == 0 || c.point.x == SIZE || c.point.y == 0 || c.point.y == SIZE);
            corners.ForEach(c => c.border = c.point.x <= 0 || c.point.x >= SIZE || c.point.y <= 0 || c.point.y >= SIZE);
            //DeleteEdgesWithVoidCorners();
            
            /* // order each centers corner list clockwise
            foreach(var center in centers)
            {
                center.OrderCornersClockwise();
            }
            
            // Set up grid
            grid = new Grid(SIZE, centers); */

        }
       
        public void RemoveArtifactCorner(Corner artifact)
        {
            // Remove the corner from all lists
            corners.Remove(artifact);
            List<Center> infectedCenters = artifact.touches;
            List<Corner> infectedCorners = artifact.adjacent;
            List<Edge> infectedEdges = artifact.protrudes;

            foreach(var center in infectedCenters)
            {
                center.corners.Remove(artifact);
            }

            foreach(var corner in infectedCorners)
            {
                corner.adjacent.Remove(artifact);
            }

            foreach(var edge in infectedEdges)
            {
                bool p = edge.v0 == artifact;

                if(p)
                {
                    edge.v0 = null;
                }
                else
                {
                    edge.v1 = null;
                }
            }

            corners.Remove(artifact);
        }
        public void DeleteEdgesWithVoidCorners()
        {
            List<Edge> voidEdges = new();
            foreach(var edge in edges)
            {
                if(edge.v0 == null || edge.v1 == null)
                {
                    voidEdges.Add(edge);
                }
            }

            foreach(var q in corners)
            {
                // remove all edges that have a void corner
                q.protrudes.RemoveAll(x => voidEdges.Contains(x));
            }
            foreach(var q in centers)
            {
                // remove all edges that have a void corner
                q.borders.RemoveAll(x => voidEdges.Contains(x));
            }
            edges.RemoveAll(x => voidEdges.Contains(x));
        }
        // This is for any voronoi corners that are outside of the map. We first go through all corners,
        // if the corner is outside of the map, we add it to a list. Then we go through the list and
        // find the coresponding edge. We then calculate the intersection point of the edge and the map border
        // and set the out of bounds corner to the intersection point.
        public Dictionary<Corner, (Vector2f, Vector2f)> outsideCornersDict; // 0 = top, 1 = bottom, 2 = right, 3 = left
        public List<Corner> outsideCorners;
        public void ClipVoronoiEdges()
        {
            Rectf bounds = new Rectf(0, 0, SIZE, SIZE);
            outsideCornersDict = new();
            

            foreach (var q in corners)
            {
                bool added = false;
                if (q.point.x < 0)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.left, bounds.top), new Vector2f(bounds.left, bounds.bottom)));
                    added = true;
                }
                else if(q.point.x > SIZE)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.right, bounds.top), new Vector2f(bounds.right, bounds.bottom)));
                    added = true;
                }
                else if(q.point.y < 0)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.left, bounds.top), new Vector2f(bounds.right, bounds.top)));
                    added = true;
                }
                else if(q.point.y > SIZE)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.left, bounds.bottom), new Vector2f(bounds.right, bounds.bottom)));
                    added = true;
                }
                if(added)
                {
                    foreach(var p in q.adjacent)
                    {
                        FindIntersection(ref p.point, q.point, outsideCornersDict[q].Item1, outsideCornersDict[q].Item2);
                    }

                }
            }
            outsideCorners = new List<Corner>();
            foreach(var q in corners)
            {
                if(q.point.x < 0 || q.point.x > SIZE || q.point.y < 0 || q.point.y > SIZE)
                {
                    outsideCorners.Add(q);
                }
            }
            // Each list corners, centers and edges contains references to related corners, we need to go through each veryOutsideCorner
            // and remove any references from all three lists.
            foreach(var q in outsideCorners)
            {
                if(q.point.x >= 0 && q.point.x <= SIZE && q.point.y >= 0 && q.point.y <= SIZE)
                {
                    UnityEngine.Debug.Log("That wasn't supposed to happen");
                    continue;
                }
                corners.Remove(q);
                List<Edge> edgesToRemove = edges.Where(x => x.v0 == q || x.v1 == q).ToList();
                // Set the edge's v0 or v1 to null, depending on which one is the outside corner
                foreach(var p in edgesToRemove)
                {
                    if(p.v0 == q)
                    {
                        p.v0 = null;
                    }
                    else if(p.v1 == q)
                    {
                        p.v1 = null;
                    }
                }
                //edges.RemoveAll(edgesToRemove.Contains);
                
                List<Center> centersToRemove = centers.Where(x => x.corners.Contains(q)).ToList();
                foreach(var p in centersToRemove)
                {
                    p.corners.Remove(q);
                }
                //centers.RemoveAll(centersToRemove.Contains);

                List<Corner> cornersToRemove = corners.Where(x => x.adjacent.Contains(q)).ToList();
                foreach(var p in cornersToRemove)
                {
                    p.adjacent.Remove(q);
                }
                //corners.RemoveAll(cornersToRemove.Contains);
            }
            /* foreach (var q in outsideCornersDict.Keys)
            {
                foreach (var p in q.adjacent)
                {
                    if(p.point.x > 0 && p.point.x < SIZE && p.point.y > 0 && p.point.y < SIZE)
                    {
                        Edge edge = LookupEdgeFromCorner(q, p);
                        if(edge != null)
                        {
                            FindIntersection(ref q.point, p.point, outsideCornersDict[q].Item1, outsideCornersDict[q].Item2);
                        }
                    }
                    else if(p.point.x < 0 && p.point.x > SIZE && p.point.y < 0 && p.point.y > SIZE)
                    {
                        veryOutsideCorners.Add(p);
                    }
                }
            } */
            /* foreach(var q in outsideCorners)
            {
                foreach(var p in q.adjacent)
                {
                    if(outsideCorners.Contains(p))
                    {
                        veryOutsideCorners.Add(q);
                        break;
                    }
                }
            }
            foreach(var q in veryOutsideCorners)
            {
                corners.Remove(q);
                List<Edge> edgesToRemove = edges.Where(x => x.v0 == q || x.v1 == q).ToList();
                edges.RemoveAll(edgesToRemove.Contains);
            } */    
            /*outsideCornersDict = new();
            foreach (var q in corners)
            {
                if (q.point.x < 0)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.left, bounds.top), new Vector2f(bounds.left, bounds.bottom)));
                }
                else if(q.point.x > SIZE)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.right, bounds.top), new Vector2f(bounds.right, bounds.bottom)));
                }
                else if(q.point.y < 0)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.left, bounds.top), new Vector2f(bounds.left, bounds.top)));
                }
                else if(q.point.y > SIZE)
                {
                    outsideCornersDict.Add(q, (new Vector2f (bounds.left, bounds.bottom), new Vector2f(bounds.left, bounds.bottom)));
                }
            }
            List<Corner> allOutsideCornersDict = new();
            foreach (var q in outsideCornersDict.Keys)
            {
                foreach (var p in q.adjacent)
                {
                    if(p.point.x > 0 && p.point.x <= SIZE && p.point.y > 0 && p.point.y < SIZE)
                    {
                        Edge edge = LookupEdgeFromCorner(q, p);
                        if(edge != null)
                        {
                            FindIntersection(ref q.point, p.point, outsideCornersDict[q].Item1, outsideCornersDict[q].Item2);
                        }
                    }
                    else
                    {
                        allOutsideCornersDict.Add(p);
                    }
                }
            }
            foreach (var q in allOutsideCornersDict)
            {
                corners.Remove(q);
            } */

        }
        // Function to find intersection point
        public static void FindIntersection(ref Vector2f outside, Vector2f inside, Vector2f boundA, Vector2f boundB)
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
                // doesn't intersect
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
                    outside = new Vector2f(x, y);
                }
                else
                {
                    // would intersect if the line was infinite
                }
            }
        }
        // Determine elevations and water at Voronoi corners. By
        // construction, we have no local minima. This is important for
        // the downslope vectors later, which are used in the river
        // construction algorithm. Also by construction, inlets/bays
        // push low elevation areas inland, which means many rivers end
        // up flowing out through them. Also by construction, lakes
        // often end up on river paths because they don't raise the
        // elevation as much as other terrain does.
        /*public void AssignCornerElevations() 
        {
            //var q:Corner, s:Corner;
            Queue<Corner> queue = new();
            
            foreach (var q in corners) {
                q.water = !Inside(q.point);
            }

            foreach (var q in corners) {
                // The edges of the map are elevation 0
                if (q.border) {
                    q.elevation = 0.0f;
                    queue.Enqueue(q);
                } else {
                    q.elevation = Mathf.Infinity;
                }
            }
            // Traverse the graph and assign elevations to each point. As we
            // move away from the map border, increase the elevations. This
            // guarantees that rivers always have a way down to the coast by
            // going downhill (no local minima).
            while (queue.Count > 0) {
                var q = queue.Dequeue();

                foreach (var s in q.adjacent) {
                    // Every step up is epsilon over water or 1 over land. The
                    // number doesn't matter because we'll rescale the
                    // elevations later.
                    var newElevation = 0.01f + q.elevation;
                    if (!q.water && !s.water) {
                        newElevation += 1;
                        /* if (needsMoreRandomness) {
                            // HACK: the map looks nice because of randomness of
                            // points, randomness of rivers, and randomness of
                            // edges. Without random point selection, I needed to
                            // inject some more randomness to make maps look
                            // nicer. I'm doing it here, with elevations, but I
                            // think there must be a better way. This hack is only
                            // used with square/hexagon grids.
                            newElevation += mapRandom.nextDouble();
                        } 
                    }
                    // If this point changed, we'll add it to the queue so
                    // that we can process its neighbors too.
                    if (newElevation < s.elevation) {
                        s.elevation = newElevation;
                        queue.Enqueue(s);
                    }
                }
            }*/
            /* foreach(var q in corners)
                UnityEngine.Debug.Log(q.elevation); 
        }*/
        public void AssignCornerElevations()
        {
            Corner q, s;
            Queue<Corner> queue = new Queue<Corner>();

            foreach (var corner in corners)
            {
                corner.water = !Inside(corner.point);
            }

            foreach (var corner in corners)
            {
                // The edges of the map are elevation 0
                if (corner.border)
                {
                    corner.elevation = 0.0f;
                    queue.Enqueue(corner);
                }
                else
                {
                    corner.elevation = float.PositiveInfinity;
                }
            }

            // Traverse the graph and assign elevations to each point.
            // As we move away from the map border, increase the elevations.
            // This guarantees that rivers always have a way down to the coast by going downhill (no local minima).
            while (queue.Count > 0)
            {
                q = queue.Dequeue();

                foreach (var adjacentCorner in q.adjacent)
                {
                    // Every step up is epsilon over water or 1 over land.
                    // The number doesn't matter because we'll rescale the elevations later.
                    float newElevation = 0.01f + q.elevation;
                    
                    if (!q.water && !adjacentCorner.water)
                    {
                        newElevation += 1;

                        // HACK: Injecting some randomness to make maps look nicer.
                        /* if (needsMoreRandomness)
                        {
                            newElevation += (float)mapRandom.NextDouble();
                        } */
                    }

                    // If this point changed, we'll add it to the queue
                    // so that we can process its neighbors too.
                    if (newElevation < adjacentCorner.elevation)
                    {
                        adjacentCorner.elevation = newElevation;
                        queue.Enqueue(adjacentCorner);
                    }
                }
            }
        }
        // Change the overall distribution of elevations so that lower
        // elevations are more common than higher
        // elevations. Specifically, we want elevation X to have frequency
        // (1-X).  To do this we will sort the corners, then set each
        // corner to its desired elevation.
        public void RedistributeElevations(List<Corner> locations) {
            // SCALE_FACTOR increases the mountain area. At 1.0 the maximum
            // elevation barely shows up on the map, so we set it to 1.1.
            float SCALE_FACTOR = 1.1f;


            locations.Sort((c1, c2) => c1.elevation.CompareTo(c2.elevation));
            for (int i = 0; i < locations.Count; i++) {
                // Let y(x) be the total area that we want at elevation <= x.
                // We want the higher elevations to occur less than lower
                // ones, and set the area to be y(x) = 1 - (1-x)^2.
                float y = i/(float)(locations.Count-1);
                // Now we have to solve for x, given the known y.
                //  *  y = 1 - (1-x)^2
                //  *  y = 1 - (1 - 2x + x^2)
                //  *  y = 2x - x^2
                //  *  x^2 - 2x + y = 0
                // From this we can use the quadratic equation to get:
                float x = Mathf.Sqrt(SCALE_FACTOR) - Mathf.Sqrt(SCALE_FACTOR*(1-y));
                if (x > 1.0) x = 1.0f;  // TODO: does this break downslopes?
                locations[i].elevation = x;
            }
        }
        // Change the overall distribution of moisture to be evenly distributed.
        public void RedistributeMoisture(List<Corner> locations)
        {
            locations.Sort((l1, l2) => l1.moisture.CompareTo(l2.moisture));

            for (int i = 0; i < locations.Count; i++)
            {
                locations[i].moisture = (float)i / (locations.Count - 1);
            }
        }
        // Determine polygon and corner types: ocean, coast, land.
        public void AssignOceanCoastAndLand() 
        {
            // Compute polygon attributes 'ocean' and 'water' based on the
            // corner attributes. Count the water corners per
            // polygon. Oceans are all polygons connected to the edge of the
            // map. In the first pass, mark the edges of the map as ocean;
            // in the second pass, mark any water-containing polygon
            // connected an ocean as ocean.
            Queue<Center> queue = new Queue<Center>();
            int numWater;
            
            foreach (var p in centers)
            {
                numWater = 0;

                foreach (var q in p.corners)
                {
                    if (q.border)
                    {
                        p.border = true;
                        p.ocean = true;
                        q.water = true;
                        queue.Enqueue(p);
                    }

                    if (q.water)
                    {
                        numWater += 1;
                    }
                }

                p.water = p.ocean || numWater >= p.corners.Count * LAKE_THRESHOLD;
            }
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();

                foreach (var r in p.neighbors)
                {
                    if (r.water && !r.ocean)
                    {
                        r.ocean = true;
                        queue.Enqueue(r);
                    }
                }
            }
            
            // Set the polygon attribute 'coast' based on its neighbors. If
            // it has at least one ocean and at least one land neighbor,
            // then this is a coastal polygon.
            foreach (var p in centers) 
            {
                int numOcean = 0;
                int numLand = 0;

                foreach (var r in p.neighbors)
                {
                    numOcean += Convert.ToInt32(r.ocean);
                    numLand += Convert.ToInt32(!r.water);
                }

                p.coast = (numOcean > 0) && (numLand > 0);
            }


            // Set the corner attributes based on the computed polygon
            // attributes. If all polygons connected to this corner are
            // ocean, then it's ocean; if all are land, then it's land;
            // otherwise it's coast.
            foreach (var q in corners)
            {
                int numOcean = 0;
                int numLand = 0;

                foreach (var p in q.touches)
                {
                    numOcean += Convert.ToInt32(p.ocean);
                    numLand += Convert.ToInt32(!p.water);
                }

                q.ocean = (numOcean == q.touches.Count);
                q.coast = (numOcean > 0) && (numLand > 0);
                q.water = q.border || ((numLand != q.touches.Count) && !q.coast);
            }
        }
        // Polygon elevations are the average of the elevations of their corners.
        public void AssignPolygonElevations() 
        {
            float sumElevation = 0f;
            foreach (var p in centers) 
            {
                sumElevation = 0.0f;
                foreach (var q in p.corners) 
                {
                    sumElevation += q.elevation;
                }
                p.elevation = sumElevation / p.corners.Count;
            }
        }
        // Calculate downslope pointers.  At every point, we point to the
        // point downstream from it, or to itself.  This is used for
        // generating rivers and watersheds.
        public void CalculateDownslopes()
        {       
            foreach (var q in corners) {
                var r = q;
                foreach (var s in q.adjacent) 
                {
                    if (s.elevation <= r.elevation) 
                    {
                        r = s;
                    }
                }
                q.downslope = r;
            }
        }
        // Calculate the watershed of every land point. The watershed is
        // the last downstream land point in the downslope graph. TODO:
        // watersheds are currently calculated on corners, but it'd be
        // more useful to compute them on polygon centers so that every
        // polygon can be marked as being in one watershed.
        public void CalculateWatersheds() 
        {
            bool changed;
            // Initially the watershed pointer points downslope one step.      
            foreach (var q in corners) 
            {
                q.watershed = q;
                if (!q.ocean && !q.coast) 
                {
                    q.watershed = q.downslope;
                }
            }
            // Follow the downslope pointers to the coast. Limit to 100
            // iterations although most of the time with numPoints==2000 it
            // only takes 20 iterations because most points are not far from
            // a coast.  TODO: can run faster by looking at
            // p.watershed.watershed instead of p.downslope.watershed.
            for (int i = 0; i < 100; i++) {
                changed = false;
                foreach (var q in corners) 
                {
                    if (!q.ocean && !q.coast && !q.watershed.coast) 
                    {
                        var r = q.downslope.watershed;
                        if (!r.ocean) 
                        {
                            q.watershed = r;
                            changed = true;
                        }
                    }
                }
                if (!changed) break;
            }
            // How big is each watershed?
            foreach (var q in corners) 
            {
                var r = q.watershed;
                r.watershed_size = 1 + (r.watershed_size ?? 0);
            }
        }
        // Create rivers along edges. Pick a random corner point, then
        // move downslope. Mark the edges and corners as rivers.
        public void CreateRivers()
        {
            for (int i = 0; i < SIZE/2; i++) 
            {
                var q = null as Corner;
                if (corners.Count > 0)
                {
                    q = corners[mapRandom.Next(0, corners.Count)];
                }
                else
                {
                    throw new InvalidOperationException("The corners list is empty. It should have been populated earlier.");
                }
                if (q.ocean || q.elevation < 0.3 || q.elevation > 0.9) continue;
                // Bias rivers to go west: if (q.downslope.x > q.x) continue;
                while (!q.coast) 
                {
                    if (q == q.downslope) 
                    {
                        break;
                    }
                    var edge = LookupEdgeFromCorner(q, q.downslope);
                    edge.river = edge.river + 1;
                    q.river = (q.river ?? 0) + 1;
                    q.downslope.river = (q.downslope.river ?? 0) + 1;  // TODO: fix double count
                    q = q.downslope;
                }
            }
        }
        // Calculate moisture. Freshwater sources spread moisture: rivers
        // and lakes (not oceans). Saltwater sources have moisture but do
        // not spread it (we set it at the end, after propagation).
        public void AssignCornerMoisture() 
        {
            float newMoisture;
            Queue<Corner> queue = new();
            // Fresh water
            foreach (var q in corners) {
                if ((q.water || q.river > 0) && !q.ocean) {
                    q.moisture = q.river > 0? Mathf.Min(3.0f, (float)(0.2f * q.river)) : 1.0f;
                    queue.Enqueue(q);
                } else {
                    q.moisture = 0.0f;
                }
                }
            while (queue.Count > 0) {
                var q = queue.Dequeue();

                foreach (var r in q.adjacent) 
                {
                    newMoisture = q.moisture * 0.9f;
                    if (newMoisture > r.moisture) 
                    {
                        r.moisture = newMoisture;
                        queue.Enqueue(r);
                    }
                }
            }
            // Salt water
            foreach (var q in corners) 
            {
                if (q.ocean || q.coast) 
                {
                    q.moisture = 1.0f;
                }
            }
        }
        // Polygon moisture is the average of the moisture at corners
        public void AssignPolygonMoisture() 
        {
            float sumMoisture;
            foreach (var p in centers) 
            {
                sumMoisture = 0.0f;
                foreach (var q in p.corners) 
                {
                    if (q.moisture > 1.0) q.moisture = 1.0f;
                    sumMoisture += q.moisture;
                }
                p.moisture = sumMoisture / p.corners.Count;
            }
        }
        // Assign a biome type to each polygon. If it has
        // ocean/coast/water, then that's the biome; otherwise it depends
        // on low/high elevation and low/medium/high moisture. This is
        // roughly based on the Whittaker diagram but adapted to fit the
        // needs of the island map generator.
        static public string GetBiome(Center p) 
        {
            if (p.ocean) {
                return "OCEAN";
            } else if (p.water) {
                if (p.elevation < 0.1) return "MARSH";
                if (p.elevation > 0.8) return "ICE";
                return "LAKE";
            } else if (p.coast) {
                return "BEACH";
            } else if (p.elevation > 0.8) {
                if (p.moisture > 0.50) return "SNOW";
                else if (p.moisture > 0.33) return "TUNDRA";
                else if (p.moisture > 0.16) return "BARE";
                else return "SCORCHED";
            } else if (p.elevation > 0.6) {
                if (p.moisture > 0.66) return "TAIGA";
                else if (p.moisture > 0.33) return "SHRUBLAND";
                else return "TEMPERATE_DESERT";
            } else if (p.elevation > 0.3) {
                if (p.moisture > 0.83) return "TEMPERATE_RAIN_FOREST";
                else if (p.moisture > 0.50) return "TEMPERATE_DECIDUOUS_FOREST";
                else if (p.moisture > 0.16) return "GRASSLAND";
                else return "TEMPERATE_DESERT";
            } else {
                if (p.moisture > 0.66) return "TROPICAL_RAIN_FOREST";
                else if (p.moisture > 0.33) return "TROPICAL_SEASONAL_FOREST";
                else if (p.moisture > 0.16) return "GRASSLAND";
                else return "SUBTROPICAL_DESERT";
            }
        }
        public void AssignBiomes()
        {
            foreach (var p in centers) 
            {
                p.biome = GetBiome(p);
            }
        }
        // Look up a Voronoi Edge object given two adjacent Voronoi
        // polygons, or two adjacent Voronoi corners
        public Edge LookupEdgeFromCenter(Center p, Center r) 
        {
            foreach (var edge in p.borders) 
            {
                if (edge.d0 == r || edge.d1 == r) return edge;
            }
            return null;
        }
        public Edge LookupEdgeFromCorner(Corner q, Corner s) 
        {
            foreach (var edge in q.protrudes) 
            {
                if (edge.v0 == s || edge.v1 == s) return edge;
            }
            return null;
        }
        // Determine whether a given point should be on the island or in the water.
        public bool Inside(Vector2f p) {
            // all points should have x and y's between 0 and 512 (SIZE) normalized point (x and y are -1 to +1)
            
            float x = 2 * (p.x / SIZE) - 1;
            float y = 2 * (p.y / SIZE) - 1;
            /* if(x > 1 || x < -1 || y > 1 || y < -1)
            {
                UnityEngine.Debug.Log(x + ", " + y);
                return false;
            } */

            return islandShape(new Vector2f(x, y));
        }
    }

    public struct Region
    {
        #region Biome Factors
        public float desiredElavation;
        public float desiredMoisture;
        public float desiredTemperature;
        #endregion

        #region Water Features
        public float riverFrequency;
        public float riverSize;
        public float lakeFrequency;
        #endregion

        // Constructor
        public Region(float desiredElavation, float desiredMoisture, float desiredTemperature, float riverFrequency, float riverSize, float lakeFrequency)
        {
            this.desiredElavation = desiredElavation;
            this.desiredMoisture = desiredMoisture;
            this.desiredTemperature = desiredTemperature;
            this.riverFrequency = riverFrequency;
            this.riverSize = riverSize;
            this.lakeFrequency = lakeFrequency;
        }
    }

    public struct Grid
    {
        public int mapSize;
        public float gridspaceSize;
        public Box[,] gridSpaces;
        public Grid(int mapSize, List<Center> centers)
        {
            this.mapSize = mapSize;
            gridspaceSize = mapSize / 10;
            
            gridSpaces = CreateGrid(mapSize);
            OrganizeCenters(centers);
        }

        public static Box[,] CreateGrid(int mapSize)
        {
            int boxSize = mapSize / 10;
            Box[,] boxes = new Box[10, 10];
            for (int x = boxSize/2; x < mapSize; x += boxSize)
            {
                int xIndex = x == 0 ? 0 : x / 100;
                for (int y = boxSize/2; y < mapSize; y += boxSize)
                {
                    int yIndex = y == 0 ? 0 : y / 100;
                    boxes[xIndex, yIndex] = new Box(new Vector2f(x, y), boxSize);
                }
            }
            return boxes;
        }
        public void OrganizeCenters(List<Center> centers)
        {
            foreach(var center in centers)
            {
                Vector2f centerPoint = center.point / 100;
                // round so that if the value is 0-99 it will be 0, 100-199 will be 1, etc.
                int x = (int)Mathf.Floor(centerPoint.x);
                int y = (int)Mathf.Floor(centerPoint.y);

                gridSpaces[x, y].centers.Add(center);
                List<Vector2f> cordinatesAdded = new List<Vector2f>();
                cordinatesAdded.Add(new Vector2f(x, y));
                foreach(var corner in center.corners)
                {
                    Vector2f cornerPoint = corner.point / 100;
                    x = (int)Mathf.Floor(centerPoint.x);
                    y = (int)Mathf.Floor(centerPoint.y);
                    if(!cordinatesAdded.Contains(new Vector2f(x, y)))
                    {
                        gridSpaces[x, y].centers.Add(center);
                        cordinatesAdded.Add(new Vector2f(x, y));
                    }
                    //UnityEngine.Debug.Log(cordinatesAdded.Count + " " + x + ", " + y);
                }
            }
        }

        public List<Center> FindCentersInGrid(int x, int y)
        {
            x = x/100;
            y = y/100;

            return gridSpaces[x, y].centers;
        }

        public class Box
        {
            public Vector2f center;
            public Vector2f[] boxCorners;
            public List<Center> centers;

            public Box(Vector2f center, float size)
            {
                this.center = center;
                boxCorners = new Vector2f[4] {
                    new Vector2f(center.x - size / 2, center.y - size / 2),
                    new Vector2f(center.x + size / 2, center.y - size / 2),
                    new Vector2f(center.x + size / 2, center.y + size / 2),
                    new Vector2f(center.x - size / 2, center.y + size / 2)
                };
                centers = new List<Center>();
            }
        }
    }
}

