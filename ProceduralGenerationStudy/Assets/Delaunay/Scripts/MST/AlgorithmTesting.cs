using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Threading.Tasks; 
using UnityEngine;

namespace MinSpanTree
{
    public class AlgorithmTesting : MonoBehaviour
    {
        public GameObject spherePrefab; // Prefab for the sphere
        public Material lineMaterial; // Material for the line renderer
        void Start()
        {
            Graph<Vector2Int> graph = new Graph<Vector2Int>(false, true); 

            Node<Vector2Int> n1 = graph.AddNode(new Vector2Int(0, 0)); 
            Node<Vector2Int> n2 = graph.AddNode(new Vector2Int(0, 3)); 
            Node<Vector2Int> n3 = graph.AddNode(new Vector2Int(3, 3)); 
            Node<Vector2Int> n4 = graph.AddNode(new Vector2Int(3, 6)); 
            Node<Vector2Int> n5 = graph.AddNode(new Vector2Int(6, 6)); 
            Node<Vector2Int> n6 = graph.AddNode(new Vector2Int(6, 9)); 
            Node<Vector2Int> n7 = graph.AddNode(new Vector2Int(9, 9)); 
            Node<Vector2Int> n8 = graph.AddNode(new Vector2Int(9, 12)); 

            graph.AddEdge(n1, n2, 999); 
            graph.AddEdge(n1, n3, 5); 
            graph.AddEdge(n2, n1, 3); 
            graph.AddEdge(n2, n4, 0); 
            graph.AddEdge(n3, n4, 12); 
            graph.AddEdge(n4, n2, 2); 
            graph.AddEdge(n4, n8, 8); 
            graph.AddEdge(n5, n4, 9); 
            graph.AddEdge(n5, n6, 2); 
            graph.AddEdge(n5, n7, 5); 
            graph.AddEdge(n5, n8, 3); 
            graph.AddEdge(n6, n7, 1); 
            graph.AddEdge(n7, n5, 4); 
            graph.AddEdge(n7, n8, 6); 
            graph.AddEdge(n8, n5, 3); 
            List<Edge<Vector2Int>> mstKruskal = graph.MinimumSpanningTreeKruskal(); 
            mstKruskal.ForEach(e => Debug.Log(e)); 
            DrawNodes(graph.Nodes);
            DrawEdges(mstKruskal);
        }

        void DrawNodes(List<Node<Vector2Int>> nodes)
        {
            foreach (Node<Vector2Int> node in nodes)
            {
                Vector3 position = new Vector3(node.Data.x, 0f, node.Data.y);
                GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                sphere.name = node.Index.ToString();
            }
        }
        void DrawEdges(List<Edge<Vector2Int>> mstKruskal)
        {
            foreach (Edge<Vector2Int> edge in mstKruskal)
            {
                Vector3 fromPosition = new Vector3(edge.From.Data.x, 0f, edge.From.Data.y);
                Vector3 toPosition = new Vector3(edge.To.Data.x, 0f, edge.To.Data.y);

                // Create a line renderer and set its positions
                GameObject lineObject = new GameObject("Line");
                LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.material = lineMaterial;
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { fromPosition, toPosition });
            }
        }


    }   
}
