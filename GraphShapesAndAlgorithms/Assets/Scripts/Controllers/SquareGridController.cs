using UnityEngine;
using Geometry;
using UnityEditor;
using System.Linq;
public class SquareGridController : MonoBehaviour
{
    [SerializeField]
    public Geometry.Grid grid;

    public int width = 4;
    public int height = 4;
    public void GenerateGrid()
    {
        grid = new Geometry.Grid(width, height);
    }
    private void OnValidate() {
        if(width < 4)
            width = 4;
        if(height < 4)
            height = 4;
        
        GenerateGrid();
    }
    public int disMany;
    private void OnDrawGizmos()
    {
        if (grid == null)
            return;

        Vector3 position = transform.position;
        float heightOffset = 0f;
         for (int i = 0; i < /* grid.vertices.Count*/ disMany; i++)
        {
            if(i >= grid.vertices.Count) continue;
             /* if(vertex.Q%2 == 0 && vertex.R%2 == 0)
                heightOffset = 1f;
            else
                heightOffset = 0f;  */
            Handles.Label(new Vector3(grid.vertices[i].Q, heightOffset, grid.vertices[i].R) + position, grid.vertices[i].Q + ", " + grid.vertices[i].R);
            Gizmos.DrawSphere(new Vector3(grid.vertices[i].Q, heightOffset, grid.vertices[i].R) + position, 0.05f);
        }

        for (int i = 0; i < /* grid.edges.Count /disMany*/2; i++)
        {
            if(i >= grid.edges.Count) continue;
            /* if(edge.Q%2 == 0 && edge.R%2 == 0)
                heightOffset = 1f;
            else
                heightOffset = 0f; */ 
            Vector3 offest = grid.edges[i].Direction == Direction.North ? new Vector3(0, 0, 0.5f) : new Vector3(0.5f, 0, 0);
            /* if(edge.isGridBounds)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.white; */
            Handles.Label(new Vector3(grid.edges[i].Q, heightOffset, grid.edges[i].R) + offest + position, grid.edges[i].Direction.ToString() + " " + grid.edges[i].Q + ", " + grid.edges[i].R);
            Gizmos.DrawLine(new Vector3(grid.edges[i].endPoints[0].Q, heightOffset, grid.edges[i].endPoints[0].R) + position, new Vector3(grid.edges[i].endPoints[1].Q, heightOffset, grid.edges[i].endPoints[1].R) + position);
        }

        for (int i = 0; i < /* grid.faces.Count */disMany; i++)
        {
            if(i >= grid.faces.Count) continue;
            Handles.Label(new Vector3(grid.faces[i].Q+0.5f, heightOffset + 0.05f, grid.faces[i].R+0.5f) + position, grid.faces[i].Q + ", " + grid.faces[i].R);
            /* foreach(Edge edge in grid.faces[i].borders)
            {
                Gizmos.DrawLine(new Vector3(edge.endPoints[0].Q, heightOffset, edge.endPoints[0].R) + position, new Vector3(edge.endPoints[1].Q, heightOffset, edge.endPoints[1].R) + position);
            }
            int z = 0;
            foreach(Vertex vertex in grid.faces[i].corners)
            {
                z++;
                Handles.Label(new Vector3(vertex.Q, heightOffset+(z/4f), vertex.R) + position, z.ToString());
                Gizmos.DrawSphere(new Vector3(vertex.Q, heightOffset, vertex.R) + position, 0.05f);
            }
            foreach(Face face in grid.faces[i].neighbors)
            {
                foreach(Edge edge in face.borders)
                {
                    Gizmos.DrawLine(new Vector3(edge.endPoints[0].Q, heightOffset, edge.endPoints[0].R) + position, new Vector3(edge.endPoints[1].Q, heightOffset, edge.endPoints[1].R) + position);
                }
            } */
            
            
        }
    }

}
