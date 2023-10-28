using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TriangleNet;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Geometry;

using MinSpanTree;
using System.Linq;

using Pathfinding;
using Unity.VisualScripting;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;
    public GameObject tempPrefab;
    public GameObject spherePrefab; // Prefab for the sphere
    public Material lineMaterial1; // Material for the line renderer
    public Material lineMaterial2; // Material for loops

    public int maxRoomWidth = 10, maxRoomHeight = 10;
    public int minRoomWidthHeight = 6;
    public int maxRooms = 10;

    public float maxHallLength = 100f;

    private GridRoomGenerator roomGenerator;
    List<Room> rooms = new();

    // Minimum Spanning Tree Variables
    Graph<Vertex> graph = new Graph<Vertex>(false, true);
    Dictionary<int, Node<Vertex>> nodes = new();
    List<Edge<Vertex>> edgesForLoops = new List<Edge<Vertex>>();
    public float percentOfEdgesToReAdd = 10;
    // Use to find the node of a room by first finding the vertex, the room number, the room number should be the key to the node list
    Dictionary<Vertex, int> roomnumberVertexDictionary = new(); 


    public GameObject vertexPrefab; // Assign a prefab to visualize vertices
    public Color edgeColor = Color.white;

    [SerializeField] private Pathfinder pathfinder;

    public static GridManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GridManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("GridManager");
                    _instance = singletonObject.AddComponent<GridManager>();
                }
            }
            return _instance;
        }
    }
    [HideInInspector] public GameObject dungeonParent;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    
    private void Start() {
        Generate();
    }
    
    public void Generate()
    {
        if(dungeonParent != null)
            DestroyImmediate(dungeonParent);
        dungeonParent = null;
        Seed.Instance.InitializeSeed();
        rooms = new();
        graph = new Graph<Vertex>(false, true);
        nodes = new();
        new List<Edge<Vertex>>();
        roomnumberVertexDictionary = new(); 

        roomGenerator = new GridRoomGenerator(maxRoomWidth, maxRoomHeight, minRoomWidthHeight, maxRooms);
        rooms = roomGenerator.GenerateRooms();

        var mesh = TriangulatePositions(rooms);
        
        List<Edge<Vertex>> mst = GenerateMST(mesh, rooms);
        //Debug.Log("Original Edge List Count: " + edgesForLoops.Count + ", MST Edge List Count: " + mst.Count);
        edgesForLoops = PickLoopEdges(edgesForLoops, mst);

        // Now we have all the rooms and we know what rooms should be connected. First, lets combine the list of edges created by MST and the recovered edges that make loops.
        List<Edge<Vertex>> allEdges = new();
        allEdges.AddRange(mst);
        allEdges.AddRange(edgesForLoops);

        
        GenerateHallwayGridMap(rooms, out Dictionary<Vector2Int, PathNode> map);
       

        // Now call the function that Picks the doors that the edge will connect its rooms from (for each edge)
        List<Hallway> hallways = roomGenerator.FixRoomConnections(allEdges, roomnumberVertexDictionary, rooms);

        MergeHallways(ref hallways);

        Pathfinder pathfinder = new();
        foreach(Hallway hallway in hallways)
        {
            hallway.Path = pathfinder.Search(hallway, map);
            if(hallway.Path.Count == 0) Debug.LogError("Path Count is 0.");
        }
        

        BuildRooms(rooms, hallways);
    }

    private void GenerateHallwayGridMap(List<Room> rooms, out Dictionary<Vector2Int, PathNode> map)
    {
        map = rooms
            .SelectMany(room => room.roomGridPositions)
            .Distinct()
            .ToDictionary(position => position, _ => new PathNode(_, true));
        Debug.Log("Map Count before entire grid "+ map.Count);
        
         // Find the bottom leftmost and top rightmost Vector2Int using lambda expressions
        Vector2Int bottomLeft = rooms.SelectMany(room => room.roomGridPositions).Aggregate((min, current) =>
        {
            return new Vector2Int(Mathf.Min(min.x, current.x), Mathf.Min(min.y, current.y));
        });
        
        Vector2Int topRight = rooms.SelectMany(room => room.roomGridPositions).Aggregate((max, current) =>
        {
            return new Vector2Int(Mathf.Max(max.x, current.x), Mathf.Max(max.y, current.y));
        });
        for(int x = bottomLeft.x; x < topRight.x; x++)
            for(int y = bottomLeft.y; y < topRight.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if(!map.ContainsKey(pos))
                    map.Add(pos, new PathNode(pos, false));
            }
    }

    private List<Hallway> MergeHallways(ref List<Hallway> hallways)
    {
        List<Hallway> hallwaysToRemove = new();
        foreach(var hallway in hallways)
        {
            if(hallwaysToRemove.Contains(hallway)) break;
            var hallwayFrom = hallway.From;
            var hallwayTo = hallway.To;
            foreach(var otherHallway in hallways)
            {
                var otherHallwayFrom = otherHallway.From;
                var otherHallwayTo = otherHallway.To;
                if(hallwayFrom == otherHallwayFrom && hallwayTo != otherHallwayTo)
                {
                    hallway.Merge(otherHallway);

                    hallwaysToRemove.Add(otherHallway);
                }
            }
        }
        foreach (Hallway hall in hallwaysToRemove)
            hallways.Remove(hall);
        return hallways;
    }
    public void BuildRooms(List<Room> rooms, List<Hallway> hallways)
    {
        GameObject dungeonParent = new();
        dungeonParent.name = "Dungeon Parent";
        this.dungeonParent = dungeonParent;
                
        foreach (Room room in rooms)
        {
            GameObject roomGo = Instantiate(tempPrefab, new Vector3(room.roomCenterPosition.x, 0, room.roomCenterPosition.y), Quaternion.identity);
            roomGo.transform.parent = dungeonParent.transform;
            roomGo.name = room.roomNumber.ToString();

            GameObject roomFloorGo = new GameObject();
            roomFloorGo.transform.position = new Vector3(room.roomCenterPosition.x, 0, room.roomCenterPosition.y);
            roomFloorGo.transform.parent = roomGo.transform;
            roomFloorGo.name = "Floor";
            roomFloorGo.AddComponent<MeshFilter>();
            roomFloorGo.AddComponent<MeshRenderer>().sharedMaterial = floorMaterial;

            GameObject roomWallGo = new GameObject();
            roomWallGo.transform.position = new Vector3(room.roomCenterPosition.x, 0, room.roomCenterPosition.y);
            roomWallGo.transform.parent = roomGo.transform;
            roomWallGo.name = "Walls";
            roomWallGo.AddComponent<MeshFilter>();
            roomWallGo.AddComponent<MeshRenderer>().sharedMaterial = wallMaterial;

            foreach(Vector2Int roomTile in room.roomGridPositions)
            {
                GameObject roomTileGo = Instantiate(tempPrefab, new Vector3(roomTile.x, -1, roomTile.y), Quaternion.identity);
                string roomName = "(" + roomTile.x + ", " + roomTile.y + ")";
                roomTileGo.name = roomName;
                roomTileGo.transform.parent = roomFloorGo.transform;
                
            }
            foreach(Vector2Int edgeTile in room.roomEdges)
            {
                if(room.doors.Contains(edgeTile)) continue;
                GameObject edgeTileGo = Instantiate(tempPrefab, new Vector3(edgeTile.x, 0, edgeTile.y), Quaternion.identity);
                string roomName = "(" + edgeTile.x + ", " + edgeTile.y + ")";
                edgeTileGo.name = roomName;
                edgeTileGo.transform.parent = roomWallGo.transform;
                edgeTileGo.GetComponent<Renderer>().sharedMaterial = wallMaterial;
            }
            
            CombineRoomMesh(roomFloorGo, Color.grey);
            CombineRoomMesh(roomWallGo, Color.black);
        }

        foreach (Hallway hallway in hallways)
        {
            GameObject hallwayGo = new();
            hallwayGo.transform.parent = dungeonParent.transform;
            hallwayGo.transform.position = new Vector3((hallway.From.x + hallway.To.x)/2, 0, (hallway.From.y + hallway.To.y)/2);
            hallwayGo.name = "Hallway " + hallway.fromRoomNumber.ToString() + " to " + hallway.toRoomNumber.ToString();
            hallwayGo.AddComponent<MeshFilter>();
            hallwayGo.AddComponent<MeshRenderer>().sharedMaterial = floorMaterial;
            foreach(Vector2Int pos in hallway.Path)
            {
                GameObject hallTileGo = Instantiate(tempPrefab, new Vector3(pos.x, 0, pos.y), Quaternion.identity);
                string name = $"Hallway: ({pos.x}, {pos.y})";
                hallTileGo.name = name;
                hallTileGo.transform.parent = hallwayGo.transform;
                hallTileGo.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            }
            CombineRoomMesh(hallwayGo, Color.grey);
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
        foreach(TriangleNet.Geometry.Edge edge in mesh.Edges)
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
    private void CombineRoomMesh(GameObject parent, Color sharedMaterialColor)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix * parent.transform.worldToLocalMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        UnityEngine.Mesh mesh = new UnityEngine.Mesh();
        mesh.CombineMeshes(combine);

        // Create a new sharedMaterial with the specified color
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = sharedMaterialColor;

        // Assign the new sharedMaterial to the parent GameObject
        parent.GetComponent<Renderer>().sharedMaterial = newMaterial;

        // Set the combined mesh
        parent.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        parent.SetActive(true);
    }
}
