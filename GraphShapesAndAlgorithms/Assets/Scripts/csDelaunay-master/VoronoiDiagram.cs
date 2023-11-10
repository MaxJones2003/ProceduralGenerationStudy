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
        bounds = new Rectf(0,0,512,512);
       
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
        map = new Map.Map(polygonNumber, iterations, stage);
        map.NewIsland(islandShape, polygonNumber, seed);
        mapCenters = map.centers;
        mapCorners = map.corners;
        mapEdges = map.edges;
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

    public int polygonsToDisplay = 1;
    void OnDrawGizmos()
    {
        if(map == null) return;

/*         foreach(var corner in map.corners)
        {
            if(corner.water)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawSphere(new Vector3(corner.point.x/100, corner.elevation, corner.point.y/100), 0.1f);
        } */

        foreach(Map.Edge edge in map.edges)
        {
            // draw a red sphere at the d0 and d1 points
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(edge.d0.point.x/100, edge.d0.elevation*5, edge.d0.point.y/100), 0.01f);
            Gizmos.DrawSphere(new Vector3(edge.d1.point.x/100, edge.d1.elevation*5, edge.d1.point.y/100), 0.01f);
            // draw a black line between d0 and d1
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(edge.d0.point.x/100, edge.d0.elevation*5, edge.d0.point.y/100), new Vector3(edge.d1.point.x/100, edge.d1.elevation*5, edge.d1.point.y/100));

            // draw a blue sphere at the v0 and v1 points
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new Vector3(edge.v0.point.x/100, edge.v0.elevation*5, edge.v0.point.y/100), 0.01f);
            Gizmos.DrawSphere(new Vector3(edge.v1.point.x/100, edge.v1.elevation*5, edge.v1.point.y/100), 0.01f);
            // draw a white line between v0 and v1
            Gizmos.color = Color.white;
            Gizmos.DrawLine(new Vector3(edge.v0.point.x/100, edge.v0.elevation*5, edge.v0.point.y/100), new Vector3(edge.v1.point.x/100, edge.v1.elevation*5, edge.v1.point.y/100));

        }
    }
    /* void OnValidate()
    {
        if(polygonsToDisplay < 1) polygonsToDisplay = 1;
        if(polygonsToDisplay > sites.Count) polygonsToDisplay = sites.Count;
    } */
}      