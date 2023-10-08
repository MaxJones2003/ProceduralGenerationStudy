using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using MinSpanTree;
using System;
public class GridRoomGenerator
{
    private int maxRoomWidth, maxRoomHeight, maxNumOfRooms, minRoomWidthHeight;
    private int numOfRooms = 0;

    // Create an instance of this script from another script
    public GridRoomGenerator(int _maxRoomWidth, int _maxRoomHeight, int _minRoomWidthHeight, int _maxNumOfRooms)
    {
        // Pass in the given variables from the GridManager so the new instance can reference the width and height of the grid
        this.maxRoomWidth = _maxRoomWidth;
        this.maxRoomHeight = _maxRoomHeight;
        this.minRoomWidthHeight = _minRoomWidthHeight;
        this.maxNumOfRooms = _maxNumOfRooms;
    }

    // This function puts everything together to place rooms on the gridspace
    public List<Room> GenerateRooms()
    {
        // Create a list of rooms, each room should contain a list of its grid positions
        List<Room> rooms = new List<Room>();
        // This dictionary will show all vector2ints from rooms proven valid and said rooms roomNumber
        Dictionary<Vector2Int, int> occupiedGridSpaces = new Dictionary<Vector2Int, int>();

        int numOfRoomsToMake = maxNumOfRooms; //Random.Range(maxNumOfRooms / 2, maxNumOfRooms);

        // Encompas in a while loops to produce rooms until the max num of rooms is produced
        int attempts = 0;
        while (numOfRooms <= numOfRoomsToMake && attempts < 10 + numOfRoomsToMake)
        {
            Room newRoom = new();

            // This allows for finding the room in the list, using the room number as the index
            newRoom.roomNumber = numOfRooms;
            newRoom.roomWidth = UnityEngine.Random.Range(minRoomWidthHeight, maxRoomWidth);
            newRoom.roomHeight = UnityEngine.Random.Range(minRoomWidthHeight, maxRoomHeight);
            // Pick a position for a room, then place its gridCells. Position must be at least maxRoomWidth away from grid width edges and at least maxRoomHeight away from grid height edges
            if (numOfRooms == 0)
            {
                // Set the first room position somewhere in bounds
                Vector2Int position = new Vector2Int(0, 0);
                newRoom.roomCenterPosition = position;
            }
            else
            {
                Room lastRoom = rooms[numOfRooms - 1];

                // Choose directions, this will make the position go up or down and left or right
                bool chooseUp = UnityEngine.Random.Range(0, 2) == 0;
                bool chooseRight = UnityEngine.Random.Range(0, 2) == 0;
                Vector2Int vertDirection = Vector2Int.zero;
                Vector2Int horzDirection = Vector2Int.zero;
                if (chooseUp) vertDirection = new Vector2Int(0, 1);
                else vertDirection = new Vector2Int(0, -1);
                if (chooseRight) horzDirection = new Vector2Int(1, 0);
                else horzDirection = new Vector2Int(-1, 0);

                int moveVertAmount = UnityEngine.Random.Range(newRoom.roomHeight / 2 + lastRoom.roomHeight / 2, newRoom.roomHeight / 2 + lastRoom.roomHeight + 10);
                int moveHorzAmount = UnityEngine.Random.Range(newRoom.roomWidth / 2 + lastRoom.roomWidth / 2, newRoom.roomWidth / 2 + lastRoom.roomWidth + 10);

                vertDirection *= moveVertAmount;
                horzDirection *= moveHorzAmount;

                Vector2Int distance = vertDirection + horzDirection;
                // Calculate a new position based on the direction and distance
                Vector2Int position = lastRoom.roomCenterPosition + distance;

                newRoom.roomCenterPosition = position;
            }
            // Room positions should be placed in a range from eachother (say at least 10 blocks away, may change based on the max room size)
            // If at any point during a room's grid position intersects with anothers, delete said room and try again elsewhere
            newRoom.roomGridPositions = SetRoomGrid(newRoom.roomCenterPosition, newRoom.roomWidth, newRoom.roomHeight);

            if (numOfRooms == 0 || CheckRoomValidity(occupiedGridSpaces, newRoom.roomGridPositions, newRoom.roomNumber))
            {
                rooms.Add(newRoom);
                foreach (Vector2Int pos in newRoom.roomGridPositions)
                {
                    occupiedGridSpaces.Add(pos, newRoom.roomNumber);
                }
                numOfRooms++;
            }
            else
            {
                attempts++;
            }
        }
        foreach (Room room in rooms)
        {
            (room.roomEdges, room.corners) = FindRoomEdgesAndCorners(room.roomGridPositions);
            room.doors = PickDoorsForRoom(room.corners);
        }

        return rooms;
    }

    // Takes the center position, width, and height of an already created room and adds on a grid for the floor space
    private List<Vector2Int> SetRoomGrid(Vector2Int roomCenter, int width, int height)
    {
        List<Vector2Int> roomGrid = new List<Vector2Int>();

        // I dont want to make squares and rectangles but will have it that way for now PLEASE CHANGE ME LATER

        for (int x = -width / 2; x < width / 2; x++)
        {
            for (int y = -height / 2; y < height / 2; y++)
            {
                roomGrid.Add(new Vector2Int(x, y) + roomCenter);
            }
        }

        return roomGrid;
    }

    // Takes the roomGridPositions list of a room and checks for overlapping Vector2Ints
    private bool CheckRoomValidity(Dictionary<Vector2Int, int> occupiedSpaces, List<Vector2Int> newRoomSpaces, int newRoomNumber)
    {
        foreach (Vector2Int pos in newRoomSpaces)
        {
            if (occupiedSpaces.ContainsKey(pos)) return false;
            else if (occupiedSpaces.ContainsKey(pos + (Vector2Int.up * 2)))
            {
                if(occupiedSpaces[pos + (Vector2Int.up * 2)] != newRoomNumber)
                {
                     return false;
                }
            } 
            else if (occupiedSpaces.ContainsKey(pos + (Vector2Int.down * 2)))
            {
                if(occupiedSpaces[pos + (Vector2Int.down * 2)] != newRoomNumber)
                {
                     return false;
                }
            }
            else if (occupiedSpaces.ContainsKey(pos + (Vector2Int.right * 2)))
            {
                if(occupiedSpaces[pos + (Vector2Int.right * 2)] != newRoomNumber)
                {
                     return false;
                }
            }
            else if (occupiedSpaces.ContainsKey(pos + (Vector2Int.left * 2)))
            {
                if(occupiedSpaces[pos + (Vector2Int.left * 2)] != newRoomNumber)
                {
                     return false;
                }
            }
        }
        return true;
    }

    // After the rooms are all set up with valid positions, this runs to find the edges of all rooms
    private (List<Vector2Int>, Dictionary<string, Vector2Int>) FindRoomEdgesAndCorners(List<Vector2Int> allSpaces)
    {
        List<Vector2Int> edges = new List<Vector2Int>();
        foreach (Vector2Int pos in allSpaces)
        {
            // Check in all directions of the position, if there are any open spaces this position is an edge
            if (!allSpaces.Contains(pos + Vector2Int.up)) edges.Add(pos);
            else if (!allSpaces.Contains(pos + Vector2Int.down)) edges.Add(pos);
            else if (!allSpaces.Contains(pos + Vector2Int.right)) edges.Add(pos);
            else if (!allSpaces.Contains(pos + Vector2Int.left)) edges.Add(pos);

        }
        // Sort the edges, starting in the top left corner and going clock wise (Maybe save the corners and doors here too)
        int lowestX = edges[0].x, highestX = edges[0].x, lowestY = edges[0].y, highestY = edges[0].y;
        foreach (Vector2Int edge in edges)
        {
            if(edge.x < lowestX) lowestX = edge.x;
            else if(edge.x > highestX) highestX = edge.x;
            
            if(edge.y < lowestY) lowestY = edge.y;
            else if(edge.y > highestY) highestY = edge.y;
        }

        // Extrapolate the top right and bottom left using the values from the other coners
        Vector2Int topLeftMostPosition = new Vector2Int(lowestX, highestY);
        Vector2Int bottomRightMostPosition = new Vector2Int(highestX, lowestY);
        Vector2Int topRightMostPosition = new Vector2Int(highestX , highestY);
        Vector2Int bottomLeftMostPosition = new Vector2Int(lowestX, lowestY);
        edges.Remove(topLeftMostPosition);
        edges.Remove(topRightMostPosition);
        edges.Remove(bottomLeftMostPosition);
        edges.Remove(bottomRightMostPosition);

        Dictionary<string, Vector2Int> corners = new Dictionary<string, Vector2Int>();
        corners.Add("TopLeft", topLeftMostPosition);
        corners.Add("TopRight", topRightMostPosition);
        corners.Add("BottomLeft", bottomLeftMostPosition);
        corners.Add("BottomRight", bottomRightMostPosition);

        return (edges, corners);
    }   
    
    private (double, double, double, double) PickDoor(Room room1, Room room2) // Returns a double for the x and y of each door position
    {
        // Go through each door and compare the distance, pick the two doors with the least distance
        float leastDistance = float.MaxValue;
        int room1Door = -1;
        int room2Door = -1;
        for(int room1DoorIndex = 0; room1DoorIndex < room1.doors.Count; room1DoorIndex++)
        {
            Vector2 room1CurrentDoorPosition = room1.doors[room1DoorIndex];
            for(int room2DoorIndex = 0; room2DoorIndex < room2.doors.Count; room2DoorIndex++)
            {
                Vector2 room2CurrentDoorPosition = room2.doors[room2DoorIndex];
                float distance = (room1CurrentDoorPosition - room2CurrentDoorPosition).magnitude;
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

    private List<Vector2Int> PickDoorsForRoom(Dictionary<string, Vector2Int> corners)
    {
        List<Vector2Int> doors = new List<Vector2Int>();

        // pick a random edge tile from each wall. Use the Room corners dictionary to define a wall
        
        Vector2Int topDoor = new Vector2Int(
            UnityEngine.Random.Range(corners["TopLeft"].x + 3, corners["TopRight"].x - 4), // Use 1 for the first value and 2 for the second because the first value already excludes that number (I dont want a door with 2 gridspaces of a corner)
            corners["TopLeft"].y
        );
        Vector2Int leftDoor = new Vector2Int(
            corners["TopLeft"].x,
            UnityEngine.Random.Range(corners["TopLeft"].y - 3, corners["BottomLeft"].y + 4)
        );
        Vector2Int bottomDoor = new Vector2Int(
            UnityEngine.Random.Range(corners["BottomLeft"].x + 3, corners["BottomRight"].x - 4), // Use 1 for the first value and 2 for the second because the first value already excludes that number (I dont want a door with 2 gridspaces of a corner)
            corners["BottomLeft"].y
        );
        Vector2Int rightDoor = new Vector2Int(
            corners["TopRight"].x,
            UnityEngine.Random.Range(corners["TopRight"].y - 3, corners["BottomRight"].y + 4)
        );

        doors.Add(topDoor);
        doors.Add(rightDoor);
        doors.Add(bottomDoor);
        doors.Add(leftDoor);
        
        return doors;
    } 

    public List<Hallway> FixRoomConnections(List<Edge<Vertex>> edges, Dictionary<Vertex, int> roomnumberVertexDictionary, List<Room> rooms)
    {
        // This function changes the from and to vertex of an edge from the center of a room to the door of a room
        List<Hallway> hallways = new List<Hallway>();
        foreach(Edge<Vertex> edge in edges)
        {
            Room room1 =  new();
            Room room2 =  new();

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
                hallway.fromRoomNumber = room1.roomNumber;
                hallway.toRoomNumber = room2.roomNumber;
                hallway.To = new Vector2Int((int)toX, (int)toY);
                hallways.Add(hallway);
            }
        }

        return hallways;
    }

}
public class Room
{
    public int roomNumber;
    public Vector2Int roomCenterPosition;
    public int roomWidth, roomHeight;
    public List<Vector2Int> roomGridPositions;
    public List<Vector2Int> roomEdges;
    public Dictionary<string, Vector2Int> corners;
    public List<Vector2Int> doors;


    // Delaunay + MST variables
    public List<Edge> connections;
    public Vertex roomVertex;
}