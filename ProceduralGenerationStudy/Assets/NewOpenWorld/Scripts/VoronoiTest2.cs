using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Voronoi;
using TriangleNet.Meshing.Algorithm;
using System.Linq;

public class VoronoiTest2 : MonoBehaviour
{
    public Vector2 blPos, trPos;
    public int numOfPoints;
    public int multiplicationFactor;

    private List<Vertex> points;

    StandardVoronoi voronoi;
    public List<Vertex> GenerateRandomPointsA(int count)
    {
        List<Vertex> points = new List<Vertex>();

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(blPos.x, trPos.x); // Generate x within the specified range
            float y = Random.Range(blPos.y, trPos.y); // Generate y within the specified range
            Vertex point = new Vertex(x, y);
            points.Add(point);
        }

        return points;
    }
            

    TriangleNet.Mesh TriangulatePositions(List<Vertex> points)
    {
        var triangulator = new Dwyer();

        var mesh = triangulator.Triangulate(points, new Configuration()) as TriangleNet.Mesh;
        Debug.Log($"Is Delaunay: {MeshValidator.IsDelaunay(mesh)}. Is Constrained Delaunay: {MeshValidator.IsConstrainedDelaunay(mesh)}.");

        return mesh;
    }

    bool a = false;
    public void VoronoiSetup()
    {
        FindObjectOfType<Seed>().InitializeSeed();

        TriangleNet.Mesh mesh = TriangulatePositions(GenerateRandomPointsA(numOfPoints));
        mesh.Renumber(NodeNumbering.CuthillMcKee);
        voronoi = new StandardVoronoi(mesh);
        
        points = mesh.Vertices.ToList();
        a = true;
    }

    private List<TriangleNet.Topology.DCEL.Vertex> ConvertVertices(List<Vertex> points)
    {
        List<TriangleNet.Topology.DCEL.Vertex> newPoints = new();

        foreach(var point in points)
        {
            TriangleNet.Topology.DCEL.Vertex vert = new(point.X, point.Y);
            newPoints.Add(vert);
        }

        return newPoints;
    }


    public void Erase()
    {
        voronoi = null;
    }

    public void OnDrawGizmos()
    {
        if(voronoi != null)
        {
            if (a) Gizmos.color = Color.red;
            else Gizmos.color = Color.blue;
            foreach(TriangleNet.Topology.DCEL.Vertex pos in voronoi.Vertices)
            {
                Vector3 pos0 = new Vector3((float)pos.X, 0, (float)pos.Y);
                Gizmos.DrawSphere(pos0, 0.4f);
            }
            Gizmos.color = Color.white;
            foreach(Edge edge in voronoi.Edges)
            {
                TriangleNet.Topology.DCEL.Vertex p0 = voronoi.GetVertexByID(edge.P0);
                TriangleNet.Topology.DCEL.Vertex p1 = voronoi.GetVertexByID(edge.P1);
                Vector3 pos0 = new Vector3((float)p0.X, 0, (float)p0.Y);
                Vector3 pos1 = new Vector3((float)p1.X, 0, (float)p1.Y);
                Gizmos.DrawLine(pos0, pos1);
            }
            for(int i = 0; i < 5; i++)
            {
                TriangleNet.Topology.DCEL.Vertex posVertex = voronoi.GetVertexByID(i);
                Vector3 pos = new Vector3((float)posVertex.X, 1, (float)posVertex.Y);
                Gizmos.DrawSphere(pos, 0.4f);
            }
        }
    }
}
