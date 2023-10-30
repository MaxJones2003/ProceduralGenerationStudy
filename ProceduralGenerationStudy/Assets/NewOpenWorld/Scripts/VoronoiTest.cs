using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class VoronoiTest : MonoBehaviour
{
    [SerializeField] private int numOfPoints = 100;
    [SerializeField] private MyVector2 blPos;
    [SerializeField] private MyVector2 trPos;

    static HashSet<VoronoiCell2> cells;
    List<VoronoiCell2> finalCells;
    private void Start()
    {
        Seed.Instance.InitializeSeed();
        GenerateShape();
    }
    public void GenerateShape()
    {
        Seed.Instance.RandomizeSeed();
        Seed.Instance.InitializeSeed();
        HashSet<MyVector2> randomPoints = GenerateRandomPoints(numOfPoints);

        // Normalize points for the Voronoi computation
        Normalizer2 normalizer = new Normalizer2(new List<MyVector2>(randomPoints));
        HashSet<MyVector2> randomSites_2d_normalized = normalizer.Normalize(randomPoints);

        // Generate Voronoi cells
        cells = DelaunayToVoronoiAlgorithm.GenerateVoronoiDiagram(randomSites_2d_normalized);

        cells = normalizer.UnNormalize(cells);

        finalCells = cells.ToList();

        finalCells.RemoveAll(cell => cell.edges.Any(edge => !IsWithinBounds(edge.p1) || !IsWithinBounds(edge.p2)));
        List<VoronoiEdge2> edges = new List<VoronoiEdge2>();
        foreach(var cell in cells) edges.AddRange(cell.edges);

        Texture2D tex = new Texture2D(512, 512);
        FindObjectOfType<RawImage>().texture = tex;


        DrawVoronoi.DrawEdges(tex, edges.ToArray(), 512, 512, blPos, trPos);
    }

    public bool quasi;
    public HashSet<MyVector2> GenerateRandomPoints(int count)
    {
        HashSet<MyVector2> points = new HashSet<MyVector2>();

        for (int i = 0; i < count; i++)
        {
            float x = 0;
            float y = 0;
            if(quasi)
            {
                x = (float)QuasirandomGenerator.GenerateRandomValue(blPos.x, trPos.x); // Generate x within the specified range
                y = (float)QuasirandomGenerator.GenerateRandomValue(blPos.y, trPos.y); // Generate y within the specified range

            }
            else
            {
                x = Random.Range(blPos.x, trPos.x); // Generate x within the specified range
                y = Random.Range(blPos.y, trPos.y); // Generate y within the specified range

            }
            MyVector2 point = new MyVector2(x, y);
            points.Add(point);
        }

        return points;
    }
    

    bool IsWithinBounds(Vector2 point)
    {
        // Check if the point's X and Y coordinates are within the defined bounds.
        return point.x >= blPos.x && point.x <= trPos.x
            && point.y >= blPos.y && point.y <= trPos.y;
    }


    
    public bool generatePress = false;
    private void OnDrawGizmos()
    {
        if(!generatePress) return;
        if(cells.Count != 0)
        {

            for(int i = 0; i < finalCells.Count; i++)
            {
                // Calculate the alpha component of the color based on the darknessValue.
                

                // Set the Gizmos.color with the modified alpha.
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(finalCells[i].sitePos.x, 0, finalCells[i].sitePos.y), finalCells[i].SignedPolygonArea()/25f);
                Gizmos.color = Color.yellow;
                int y = 0;
                foreach(VoronoiEdge2 edge in finalCells[i].edges)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(new Vector3(edge.p1.x, y, edge.p1.y), new Vector3(edge.p2.x, y, edge.p2.y));
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(new Vector3(edge.p1.x, y, edge.p1.y), 0.005f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(new Vector3(edge.p2.x, y, edge.p2.y), 0.005f);
                }
            }

        }
        // Draw bounds
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(blPos.x, 0, blPos.y), .5f);
        Gizmos.DrawSphere(new Vector3(trPos.x, 0, trPos.y), .5f);
        Gizmos.DrawSphere(new Vector3(blPos.x, 0, trPos.y), .5f);
        Gizmos.DrawSphere(new Vector3(trPos.x, 0, blPos.y), .5f);

        Gizmos.DrawLine(new Vector3(blPos.x, 0, blPos.y), new Vector3(blPos.x, 0, trPos.y));
        Gizmos.DrawLine(new Vector3(blPos.x, 0, trPos.y), new Vector3(trPos.x, 0, trPos.y));
        Gizmos.DrawLine(new Vector3(trPos.x, 0, trPos.y), new Vector3(trPos.x, 0, blPos.y));
        Gizmos.DrawLine(new Vector3(trPos.x, 0, blPos.y), new Vector3(blPos.x, 0, blPos.y));
    }

    
}
