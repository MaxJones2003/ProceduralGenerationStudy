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

    private void OnDrawGizmos()
    {
        if (grid == null)
            return;

        Vector3 position = transform.position;
        float heightOffset = 0f;
        foreach (Vertex vertex in grid.vertices)
        {
            if(vertex.Q%2 == 0 && vertex.R%2 == 0)
                heightOffset = 1f;
            else
                heightOffset = 0f;
            Handles.Label(new Vector3(vertex.Q, heightOffset, vertex.R) + position, vertex.Q + ", " + vertex.R);
            Gizmos.DrawSphere(new Vector3(vertex.Q, heightOffset, vertex.R) + position, 0.05f);
        }

        foreach (Edge edge in grid.edges)
        {
            if(edge.Q%2 == 0 && edge.R%2 == 0)
                heightOffset = 1f;
            else
                heightOffset = 0f;
            Vector3 offest = edge.Direction == Direction.North ? new Vector3(0, 0, 0.5f) : new Vector3(0.5f, 0, 0);
            /* if(edge.isGridBounds)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.white; */
            Handles.Label(new Vector3(edge.Q, heightOffset, edge.R) + offest + position, edge.Direction.ToString() + " " + edge.Q + ", " + edge.R);
            Gizmos.DrawLine(new Vector3(edge.endPoints[0].Q, heightOffset, edge.endPoints[0].R) + position, new Vector3(edge.endPoints[1].Q, heightOffset, edge.endPoints[1].R) + position);
        }

        foreach(Face face in grid.faces)
        {

            Handles.Label(new Vector3(face.Q+0.5f, 0.05f, face.R+0.5f) + position, face.Q + ", " + face.R);
            foreach(Face neighbor in face.neighbors)
            {
                /* if(neighbor.isOutOfGrid)
                {
                    Handles.color = Color.red;
                    Handles.Label(new Vector3(neighbor.Q+0.5f, 1f, neighbor.R+0.5f) + position, neighbor.Q + ", " + neighbor.R + " is out of grid");
                } */
                
            }
            
        }
    }

}
