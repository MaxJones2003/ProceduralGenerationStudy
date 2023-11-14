using UnityEngine;
using System.Collections.Generic;
 
using csDelaunay;
using System.Linq;
using Map;

public class VoronoiDiagram : MonoBehaviour {
 
    // The number of polygons/sites we want
    public IslandShapeEnum islandShape = IslandShapeEnum.Radial;
    public int polygonNumber = 200;
    public int iterations = 5;
 
    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private Dictionary<Vector2f, Site> boundedSites;
    private List<csDelaunay.Edge> edges;
    int seed;
    Voronoi voronoi;
    Rectf bounds;
    Map.Map map;
 
    /* public void GenerateVoronoi() {
        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = CreateRandomPoints();
       
        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        
       
        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        voronoi = new Voronoi(points,bounds,iterations);

 
        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);
 
        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;
        FindBoundSites();
        DisplayVoronoiDiagram();
    } */
    public int stage;

    public List<Map.Center> mapCenters;
    public List<Map.Corner> mapCorners;
    public List<Map.Edge> mapEdges;
    public void GenerateVoronoi()
    {
        bounds = new Rectf(0,0,512,512);
        map = new Map.Map(polygonNumber, 512, iterations, stage);
        map.NewIsland(islandShape, polygonNumber, seed);
        mapCenters = map.centers;
        mapCorners = map.corners;
        mapEdges = map.edges;

        if(displayPointAtIndex < 0) displayPointAtIndex = 0;
        if(displayPointAtIndex >= mapCorners.Count) displayPointAtIndex = mapCorners.Count - 1;

        currentCorner = mapCorners[displayPointAtIndex];
        displayCorners = currentCorner.adjacent;
        displayEdges = currentCorner.protrudes;

        currentCenter = mapCenters[displayPointAtIndex];
        displayCenters = currentCenter.neighbors;
        displayCenterEdges = currentCenter.borders;
    }
    public List<Vector2f> CreateRandomPoints() {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++) {
            points.Add(new Vector2f(Random.Range(0,512), Random.Range(0,512)));
        }
 
        return points;
    }
 
    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(512,512);
        foreach (KeyValuePair<Vector2f,Site> kv in sites) {
            DrawLargePixel(tx, (int)kv.Key.x, (int)kv.Key.y, Color.red, 1);
        }
        foreach (csDelaunay.Edge edge in edges) {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;
 
            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }
        tx.Apply();
 
        GetComponent<Renderer>().sharedMaterial.mainTexture = tx;
    }
 
   
    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0) {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;
        
       
        int dx = Mathf.Abs(x1-x0);
        int dy = Mathf.Abs(y1-y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx-dy;
       
        while (true) {
            tx.SetPixel(x0+offset,y0+offset,c);
           
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2*err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
        DrawLargePixel(tx, (int)p0.x, (int)p0.y, Color.blue, 1);
        DrawLargePixel(tx, (int)p1.x, (int)p1.y, Color.blue, 1);
    }
    private void DrawLargePixel(Texture2D tx, int x, int y, Color color, int size)
    {
        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                tx.SetPixel(x + i, y + j, color);
            }
        }
    }

    public void FindBoundSites()
    {
        
        boundedSites = new();

        foreach(var siteDic in sites)
        {
            Site site = siteDic.Value;
            foreach(csDelaunay.Edge edge in site.Edges)
            {
                if(edge.ClippedEnds == null) continue;

                if(BoundsCheck.Check(edge.ClippedEnds[LR.LEFT], bounds) != 0)
                {
                    boundedSites.Add(siteDic.Key, site);
                    break;
                }
                else if(BoundsCheck.Check(edge.ClippedEnds[LR.RIGHT], bounds) != 0)
                {
                    boundedSites.Add(siteDic.Key, site);
                    break;
                }
            }
        }
    }

    #region Gizmos
    void OnDrawGizmos()
    {
        if(map == null) return;

        foreach(var corner in map.corners)
        {
            if(corner.border)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawSphere(new Vector3(corner.point.x/100, corner.elevation, corner.point.y/100), 0.01f);
        }

        // only draw the edges that are within the border
        
        foreach(Map.Edge edge in map.edges)
        {
            // draw a red sphere at the d0 and d1 points
            /* Gizmos.color = Color.red;

            Gizmos.DrawSphere(new Vector3(edge.d0.point.x/100, edge.d0.elevation, edge.d0.point.y/100), 0.01f);

            Gizmos.DrawSphere(new Vector3(edge.d1.point.x/100, edge.d1.elevation, edge.d1.point.y/100), 0.01f); */
            // draw a black line between d0 and d1
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(edge.d0.point.x/100, edge.d0.elevation, edge.d0.point.y/100), new Vector3(edge.d1.point.x/100, edge.d1.elevation, edge.d1.point.y/100));

            // draw a blue sphere at the v0 and v1 points
            /* Gizmos.color = Color.blue;
            if(edge.v0.border)
                Gizmos.DrawSphere(new Vector3(edge.v0.point.x/100, edge.v0.elevation, edge.v0.point.y/100), 0.01f);
            if(edge.v1.border)
                Gizmos.DrawSphere(new Vector3(edge.v1.point.x/100, edge.v1.elevation, edge.v1.point.y/100), 0.01f); */
            // draw a white line between v0 and v1
            if(edge.v0 != null && edge.v1 != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(new Vector3(edge.v0.point.x/100, edge.v0.elevation, edge.v0.point.y/100), new Vector3(edge.v1.point.x/100, edge.v1.elevation, edge.v1.point.y/100));
            }

        }

        // Draw a boundry with the Rectf bounds it has a float for the top bottom left and right

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(bounds.left/100, 0, bounds.top/100), new Vector3(bounds.right/100, 0, bounds.top/100));
        Gizmos.DrawLine(new Vector3(bounds.left/100, 0, bounds.bottom/100), new Vector3(bounds.right/100, 0, bounds.bottom/100));
        Gizmos.DrawLine(new Vector3(bounds.left/100, 0, bounds.top/100), new Vector3(bounds.left/100, 0, bounds.bottom/100));
        Gizmos.DrawLine(new Vector3(bounds.right/100, 0, bounds.top/100), new Vector3(bounds.right/100, 0, bounds.bottom/100));

        /* Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(currentCorner.point.x/100, currentCorner.elevation, currentCorner.point.y/100), 0.02f);

        Gizmos.color = Color.yellow;
        foreach(var corner in displayCorners)
        {
            Gizmos.DrawSphere(new Vector3(corner.point.x/100, corner.elevation, corner.point.y/100), 0.015f);
        }
        Gizmos.color = Color.cyan;
        foreach(var edge in displayEdges)
        {
            Gizmos.DrawLine(new Vector3(edge.v0.point.x/100, edge.v0.elevation, edge.v0.point.y/100), new Vector3(edge.v1.point.x/100, edge.v1.elevation, edge.v1.point.y/100));
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(currentCenter.point.x/100, currentCenter.elevation, currentCenter.point.y/100), 0.02f);

        Gizmos.color = Color.yellow;
        foreach(var center in displayCenters)
        {
            Gizmos.DrawSphere(new Vector3(center.point.x/100, center.elevation, center.point.y/100), 0.015f);
        }
        Gizmos.color = Color.cyan;
        foreach(var edge in displayCenterEdges)
        {
            Gizmos.DrawLine(new Vector3(edge.v0.point.x/100, edge.v0.elevation, edge.v0.point.y/100), new Vector3(edge.v1.point.x/100, edge.v1.elevation, edge.v1.point.y/100));
        } */
    }
    public int displayPointAtIndex = 0;
    private Corner currentCorner;
    private List<Corner> displayCorners = new List<Corner>();
    private Center currentCenter;
    private List<Center> displayCenters = new List<Center>();
    private List<Map.Edge> displayCenterEdges = new List<Map.Edge>();
    private List<Map.Edge> displayEdges = new List<Map.Edge>();
    /* void OnValidate()
    {
        if(displayPointAtIndex < 0) displayPointAtIndex = 0;
        if(displayPointAtIndex >= mapCorners.Count) displayPointAtIndex = mapCorners.Count - 1;

        currentCorner = mapCorners[displayPointAtIndex];
        displayCorners = currentCorner.adjacent;
        displayEdges = currentCorner.protrudes;

        currentCenter = mapCenters[displayPointAtIndex];
        displayCenters = currentCenter.neighbors;
        displayCenterEdges = currentCenter.borders;
    } */
    #endregion
}      