using System.Linq;
using UnityEngine;

[System.Serializable]
public class SquareGrid
{
    public int[,] coordinates { get; set; }
    public SquareFace[,] faces { get; set; }
    public SquareEdge[,] edges { get; set; }
    public SquareCorner[,] corners { get; set; }

    public SquareGrid(int width, int height)
    {
        this.coordinates = new int[width, height];
        (int numberOfFacesWidth, int numberOfFacesHeight) = CalculateNumberOfFaceElements(width, height);
        this.faces = new SquareFace[numberOfFacesWidth, numberOfFacesHeight];
        this.edges = new SquareEdge[width, height];
        this.corners = new SquareCorner[width, height];
    }

    private (int, int) CalculateNumberOfFaceElements(int width, int height)
    {
        return ((width - 1) * (height - 1) / width, (width - 1) * (height - 1) / height);
    }

    public void CreateGrid()
    {
        CreateCorners();
        CreateEdges();
        //CreateFaces();
    }
    
    private void CreateCorners()
    {
        for (int q = 0; q < coordinates.GetLength(1); q++)
        {
            for (int r = 0; r < coordinates.GetLength(0); r++)
            {
                corners[q, r] = new SquareCorner(q, r);
            }
        }
    }

    private void CreateEdges()
    {
        // This leaves the right most verticle edges null and the bottom most horizontal edges null
        for (int r = coordinates.GetLength(1)-1; r < 1; r--)
        {
            for (int q = 0; q < coordinates.GetLength(0)-1; q++)
            {
                edges[q, r] = new SquareEdge(corners[q, r], corners[q + 1, r]);
                edges[q, r] = new SquareEdge(corners[q, r], corners[q, r - 1]);
            }
        }
        // Add the bottom most horizontal edges
        for(int q = 0; q < coordinates.GetLength(0)-2; q++)
        {
            edges[q, 0] = new SquareEdge(corners[q, 0], corners[q + 1, 0]);
        }
        // Add the right most verticle edges
        for (int r = coordinates.GetLength(1)-1; r < 1; r--)
        {
            int q = coordinates.GetLength(0)-1;
            edges[q, r] = new SquareEdge(corners[q, r], corners[q, r - 1]);
        }

    }

  /*   private void CreateFaces()
    {
        for (int q = 0; q < faces.GetLength(1); q++)
        {
            for (int r = 0; r < faces.GetLength(0); r++)
            {
                faces[q, r] = new SquareFace(corners[q, r], corners[q + 1, r], corners[q, r - 1], corners[q + 1, r - 1]);
            }
        }
    } */

    // draw the grid in the editor for debugging
    public void DrawGrid()
    {
        foreach(SquareCorner corner in corners)
        {
            Vector3 position = new Vector3(corner.Q, 0, corner.R);
            Gizmos.DrawSphere(position, 0.1f);
        }
        foreach(SquareEdge edge in edges.Cast<SquareEdge>())
        {
            SquareCorner endpoint1 = (SquareCorner)edge.Endpoints[0];
            SquareCorner endpoint2 = (SquareCorner)edge.Endpoints[1];
            Vector3 position = new(endpoint1.Q, 0, endpoint1.R);
            Vector3 position2 = new(endpoint2.Q, 0, endpoint2.R);
            Gizmos.DrawLine(position, position2);
        }
    }
    
}
