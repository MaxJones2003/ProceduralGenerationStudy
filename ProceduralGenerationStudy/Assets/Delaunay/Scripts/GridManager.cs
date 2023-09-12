using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TriangleNet;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

using MinSpanTree;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public GameObject tempPrefab;
    public GameObject spherePrefab; // Prefab for the sphere
    public Material lineMaterial1; // Material for the line renderer
    public Material lineMaterial2; // Material for loops

    public int gridWidth = 100;
    public int gridHeight = 100;
    public int maxRoomWidth = 10, maxRoomHeight = 10;
    public int minRoomWidthHeight = 6;
    public int maxRooms = 10;

    public float maxHallLength = 100f;

    private GridRoomGenerator roomGenerator;
    List<Room> rooms = new List<Room>();

    // Minimum Spanning Tree Variables
    Graph<Vertex> graph = new Graph<Vertex>(false, true); 
    Dictionary<int, Node<Vertex>> nodes = new Dictionary<int, Node<Vertex>>();
    List<Edge<Vertex>> edgesForLoops = new List<Edge<Vertex>>();
    public float percentOfEdgesToReAdd = 10;
    // Use to find the node of a room by first finding the vertex, the room number, the room number should be the key to the node list
    Dictionary<Vertex, int> roomnumberVertexDictionary = new Dictionary<Vertex, int>(); 


    public GameObject vertexPrefab; // Assign a prefab to visualize vertices
    public Color edgeColor = Color.white;
    void Start()
    {
        roomGenerator = new GridRoomGenerator(gridWidth, gridHeight, maxRoomWidth, maxRoomHeight, minRoomWidthHeight, maxRooms);
        rooms = roomGenerator.GenerateRooms();

        GenerateRoom(rooms);
        var mesh = TriangulatePositions(rooms);
        
        List<Edge<Vertex>> mst = GenerateMST(mesh, rooms);
        //Debug.Log("Original Edge List Count: " + edgesForLoops.Count + ", MST Edge List Count: " + mst.Count);
        edgesForLoops = PickLoopEdges(edgesForLoops, mst);

        // Now we have all the rooms and we know what rooms should be connected. First, lets combine the list of edges created by MST and the recovered edges that make loops.
        List<Edge<Vertex>> allEdges = new List<Edge<Vertex>>();
        allEdges.AddRange(mst);
        allEdges.AddRange(edgesForLoops);

        // Now call the function that Picks the doors that the edge will connect its rooms from (for each edge)
        List<Hallway> hallways = FixRoomConnections(allEdges);
        //hallways = DetermineMultiConnectionHallways(hallways);

        DrawNodes(graph.Nodes);
        //DrawEdges(allEdges, lineMaterial1);
        DrawHallwaysNew(hallways, lineMaterial2);
        //DrawEdges(edgesForLoops, lineMaterial2);
    }
    public void GenerateRoom(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            

            GameObject roomGo = Instantiate(tempPrefab, new Vector3(room.roomCenterPosition.x, 0, room.roomCenterPosition.y), Quaternion.identity);
            roomGo.name = room.roomNumber.ToString();
            foreach(Vector2Int roomTile in room.roomGridPositions)
            {
                GameObject roomTileGo = Instantiate(tempPrefab, new Vector3(roomTile.x, -1, roomTile.y), Quaternion.identity);
                string roomName = "(" + roomTile.x + ", " + roomTile.y + ")";
                roomTileGo.name = roomName;
                roomTileGo.transform.parent = roomGo.transform;
                
            }
            foreach(Vector2Int edgeTile in room.roomEdges)
            {
                GameObject edgeTileGo = Instantiate(tempPrefab, new Vector3(edgeTile.x, 0, edgeTile.y), Quaternion.identity);
                string roomName = "(" + edgeTile.x + ", " + edgeTile.y + ")";
                edgeTileGo.name = roomName;
                edgeTileGo.transform.parent = roomGo.transform;
            }
            //Instantiate(tempPrefab, new Vector3(room.corners["TopLeft"]. x, 0, room.corners["TopLeft"].y), Quaternion.identity);
            foreach (Vector2Int door in room.doors)
            {
                GameObject doorGo = Instantiate(tempPrefab, new Vector3(door.x, 1, door.y), Quaternion.identity);
                string roomName = "(" + door.x + ", " + door.y + ")";
                doorGo.name = roomName;
                doorGo.transform.parent = roomGo.transform;
            }
        }
    }   
    public TriangleNet.Meshing.IMesh TriangulatePositions(List<Room> rooms)
    {
        // Step 1: Create a polygon with super triangle vertices
        var points = new List<Vertex>();
        foreach (Room room in rooms)
        {
            Vertex roomVertex = (new Vertex(room.roomCenterPosition.x, room.roomCenterPosition.y));
            // Add the room center positions to the nodes list
            nodes.Add(room.roomNumber ,graph.AddNode(roomVertex));
            points.Add(roomVertex);
            room.roomVertex = roomVertex;
            roomnumberVertexDictionary.Add(roomVertex, room.roomNumber);
        }

        var triangulator = new Dwyer();

        var mesh = triangulator.Triangulate(points, new Configuration());
        int x = 0;
        foreach(Vertex vertex in mesh.Vertices)
        {
            roomnumberVertexDictionary.Remove(rooms[x].roomVertex);
            roomnumberVertexDictionary.Add(vertex, rooms[x].roomNumber);
            x++;
        }

        return mesh;
    }

    private List<Edge<Vertex>> GenerateMST(TriangleNet.Meshing.IMesh mesh, List<Room> rooms)
    {
        // Find all the edges, take note of each ones point int value (P0, P1)
        List<int> edgePointIndexList = new List<int>();
        foreach(Edge edge in mesh.Edges)
        {
            edgePointIndexList.Add(edge.P0);
            edgePointIndexList.Add(edge.P1);
        }
        // Find the vertex position of each edge by the (P0, P1) values and make a MST Edge
        for(int i = 0; i < edgePointIndexList.Count; i += 2)
        {
            // Use P0 and P1 to find the Vertex from mesh.Vertices
            Vertex fromVertex = new Vertex();
            Vertex toVertex = new Vertex();
            foreach (Vertex vertex in mesh.Vertices)
            {
                if(vertex.ID == edgePointIndexList[i])
                {
                    fromVertex = vertex;
                    break;
                }
            }
            foreach (Vertex vertex in mesh.Vertices)
            {
                if (vertex.ID == edgePointIndexList[i+1])
                {
                    toVertex = vertex;
                    break;
                }
            }
            if(Vector2.Distance(new Vector2((float)fromVertex.x, (float)fromVertex.y), new Vector2((float)toVertex.x, (float)toVertex.y)) < maxHallLength)
            {
                // Find the nodes on the graph
                int p0NodeKey = roomnumberVertexDictionary[fromVertex];
                int p1NodeKey = roomnumberVertexDictionary[toVertex];

                // Create a new edge with this info
                graph.AddEdge(nodes[p0NodeKey], nodes[p1NodeKey], 0);
            }            
        }
        
        foreach(Edge<Vertex> edge in graph.GetEdges())
        {
            edge.WeighVector2Edge(maxHallLength, new Vector2((float)edge.From.Data.X, (float)edge.From.Data.y), new Vector2((float)edge.To.Data.X, (float)edge.To.Data.y));
        }

        // Now make a duplicate list that will be used to determine what edges will be added back to form loops
        edgesForLoops = graph.GetEdges();

        return graph.MinimumSpanningTreeKruskal();
    }

    private List<Edge<Vertex>> PickLoopEdges(List<Edge<Vertex>> edges, List<Edge<Vertex>> mstEdges)
    {
        foreach (Edge<Vertex> edge in edges)
        {
            if(mstEdges.Contains(edge))
            {
                edges.Remove(edge);
            }
        }
        List<Edge<Vertex>> addEdges = new List<Edge<Vertex>>();
        int numToChoose = (int)(edges.Count / percentOfEdgesToReAdd);
        for(int i = 0; i < numToChoose; i++)
        {
            int index = Random.Range(0, edges.Count);
            addEdges.Add(edges[index]);
            edges.Remove(edges[index]);
        }

        return addEdges;
    }

    private List<Hallway> FixRoomConnections(List<Edge<Vertex>> edges)
    {
        // This function changes the from and to vertex of an edge from the center of a room to the door of a room
        List<Hallway> hallways = new List<Hallway>();
        foreach(Edge<Vertex> edge in edges)
        {
            Debug.Log(edge);
            Room room1 =  new Room();
            Room room2 =  new Room();

            if(roomnumberVertexDictionary.ContainsKey(edge.From.Data))
            {
                room1 = rooms[roomnumberVertexDictionary[edge.From.Data]];
            }
            if(roomnumberVertexDictionary.ContainsKey(edge.To.Data))
            {
                room2 = rooms[roomnumberVertexDictionary[edge.To.Data]];
            }

            if(room1 != null && room2 != null)
            {
                double fromX, fromY, toX, toY;
                (fromX, fromY, toX, toY) = PickDoor(room1, room2);
                Hallway hallway = new Hallway();
                hallway.From = new Vector2Int((int)fromX, (int)fromY);
                hallway.To = new Vector2Int((int)toX, (int)toY);
                hallways.Add(hallway);
            }
        }

        return hallways;
    }
    private (double, double, double, double) PickDoor(Room room1, Room room2) // Returns a double for the x and y of each door position
    {
        // Go through each door and compare the distance, pick the two doors with the least distance
        float leastDistance = 100000000;
        int room1Door = 0;
        int room2Door = 0;
        for(int room1DoorIndex = 0; room1DoorIndex < room1.doors.Count; room1DoorIndex++)
        {
            Vector2 room1CurrentDoorPosition = room1.doors[room1DoorIndex];
            for(int room2DoorIndex = 0; room2DoorIndex < room2.doors.Count; room2DoorIndex++)
            {
                Vector2 room2CurrentDoorPosition = room2.doors[room2DoorIndex];
                float distance = Vector2.Distance(room1CurrentDoorPosition, room2CurrentDoorPosition);
                if(distance < leastDistance)
                {
                    leastDistance = distance;
                    room1Door = room1DoorIndex;
                    room2Door = room2DoorIndex;
                    //Debug.Log(room1Door + " " + room2Door);
                }
            }
        }

        double x1 = room1.doors[room1Door].x;
        double y1 = room1.doors[room1Door].y;
        double x2 = room2.doors[room2Door].x;
        double y2 = room2.doors[room2Door].y;

        return (x1, y1, x2, y2);
    }
    private List<Hallway> DetermineMultiConnectionHallways(List<Hallway> hallways)
    {
        // Create a new dictionary of hallways with the "From" as the key
        Dictionary<Vector2Int, Hallway> hallwayDictionary = new Dictionary<Vector2Int, Hallway>();
        
        foreach (Hallway hallway in hallways)
        {
            if (hallwayDictionary.ContainsKey(hallway.From))
            {
                // Merge the current hallway with the existing one if a key collision occurs
                Hallway existingHallway = hallwayDictionary[hallway.From];
                // Remove the existing hallway by key, then re-add the modified one
                hallwayDictionary.Remove(existingHallway.From);
                // Merge the existing hallway with the new one
                existingHallway.Merge(hallway);
            }
            else
            {
                // Add the hallway to the dictionary if the key doesn't exist
                hallwayDictionary.Add(hallway.From, hallway);
            }
        }

        // Convert the dictionary values back to a list
        List<Hallway> mergedHallways = new List<Hallway>(hallwayDictionary.Values);

        return mergedHallways;
    }
    private void DrawNodes(List<Node<Vertex>> nodes)
    {
        foreach (Node<Vertex> node in nodes)
        {
            Vector3 position = new Vector3((float)node.Data.x, 1f, (float)node.Data.y);
            GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
            sphere.name = node.Index.ToString();
        }
    }
    private void DrawEdges(List<Edge<Vertex>> edges, Material lineMaterial)
    {
        foreach (Edge<Vertex> edge in edges)
        {
            Vector3 fromPosition = new Vector3((float)edge.From.Data.x, 0f, (float)edge.From.Data.y);
            Vector3 toPosition = new Vector3((float)edge.To.Data.x, 0f, (float)edge.To.Data.y);

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
    private void DrawHallways(List<Hallway> hallways, Material lineMaterial)
    {
        foreach (Hallway hallway in hallways)
        {
            Vector3 fromPosition = new Vector3(hallway.From.x, 0f, hallway.From.y);
            Vector3 toPosition = new Vector3(hallway.To.x, 0f, hallway.To.y);

            // Create a line renderer and set its positions
            GameObject lineObject = new GameObject("HallwayLine");
            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            if (hallway.MultiConnectionHall)
            {
                // Calculate the midpoint between From and To
                Vector2Int midpoint = new Vector2Int(
                    (hallway.From.x + hallway.To.x) / 2,
                    (hallway.From.y + hallway.To.y) / 2
                );

                Vector3 midpointPosition = new Vector3(midpoint.x, 0f, midpoint.y);

                // Create two lines: From to midpoint and midpoint to merged To
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { fromPosition, midpointPosition });

                GameObject secondLineObject = new GameObject("HallwayLine");
                LineRenderer secondLineRenderer = secondLineObject.AddComponent<LineRenderer>();
                secondLineRenderer.material = lineMaterial;
                secondLineRenderer.startWidth = 0.1f;
                secondLineRenderer.endWidth = 0.1f;
                secondLineRenderer.positionCount = 2;
                secondLineRenderer.SetPositions(new Vector3[] { midpointPosition, toPosition });
            }
            else
            {
                // For non-multi-connection hallways, just draw a line from From to To
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { fromPosition, toPosition });
            }
        }
    }

    private void DrawHallwaysNew(List<Hallway> hallways, Material lineMaterial)
    {
        foreach (Hallway hallway in hallways)
        {
            if (hallway.MultiConnectionHall)
            {
                // If it's a multi-connection hallway with at least 2 points, create lines between them
                for (int i = 0; i < hallway.MultiConnectionPointList.Count - 1; i++)
                {
                    Vector3 fromPosition = new Vector3(hallway.MultiConnectionPointList[i].x, 1f, hallway.MultiConnectionPointList[i].y);
                    Vector3 toPosition = new Vector3(hallway.MultiConnectionPointList[i + 1].x, 1f, hallway.MultiConnectionPointList[i + 1].y);

                    // Create a line renderer and set its positions
                    GameObject lineObject = new GameObject("HallwayLine");
                    LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
                    lineRenderer.material = lineMaterial;
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.endWidth = 0.1f;
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPositions(new Vector3[] { fromPosition, toPosition });
                }
            }
            else
            {
                // For non-multi-connection hallways, just draw a line from From to To
                Vector3 fromPosition = new Vector3(hallway.From.x, 1f, hallway.From.y);
                Vector3 toPosition = new Vector3(hallway.To.x, 1f, hallway.To.y);

                // Create a line renderer and set its positions
                GameObject lineObject = new GameObject("HallwayLine");
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
