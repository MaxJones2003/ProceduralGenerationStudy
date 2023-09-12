using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
public class GridRoomGenerator
{
    private int width, height, maxRoomWidth, maxRoomHeight, maxNumOfRooms, minRoomWidthHeight;
    private int numOfRooms = 0;

    // Create an instance of this script from another script
    public GridRoomGenerator(int _width, int _height, int _maxRoomWidth, int _maxRoomHeight, int _minRoomWidthHeight, int _maxNumOfRooms)
    {
        // Pass in the given variables from the GridManager so the new instance can reference the width and height of the grid
        this.width = _width;
        this.height = _height;
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
            Room newRoom = new Room();

            // This allows for finding the room in the list, using the room number as the index
            newRoom.roomNumber = numOfRooms;
            newRoom.roomWidth = Random.Range(minRoomWidthHeight, maxRoomWidth);
            newRoom.roomHeight = Random.Range(minRoomWidthHeight, maxRoomHeight);
            // Pick a position for a room, then place its gridCells. Position must be at least maxRoomWidth away from grid width edges and at least maxRoomHeight away from grid height edges
            if (numOfRooms == 0)
            {
                // Set the first room position somewhere in bounds
                Vector2Int position = new Vector2Int(Random.Range(newRoom.roomWidth / 2, width - newRoom.roomWidth / 2), Random.Range(newRoom.roomHeight / 2, height - newRoom.roomHeight / 2));
                newRoom.roomCenterPosition = position;
            }
            else
            {
                Room lastRoom = rooms[numOfRooms - 1];

                // Choose directions, this will make the position go up or down and left or right
                bool chooseUp = Random.Range(0, 2) == 0;
                bool chooseRight = Random.Range(0, 2) == 0;
                Vector2Int vertDirection = Vector2Int.zero;
                Vector2Int horzDirection = Vector2Int.zero;
                if (chooseUp) vertDirection = new Vector2Int(0, 1);
                else vertDirection = new Vector2Int(0, -1);
                if (chooseRight) horzDirection = new Vector2Int(1, 0);
                else horzDirection = new Vector2Int(-1, 0);

                int moveVertAmount = Random.Range(newRoom.roomHeight / 2 + lastRoom.roomHeight / 2, newRoom.roomHeight / 2 + lastRoom.roomHeight + 20);
                int moveHorzAmount = Random.Range(newRoom.roomWidth / 2 + lastRoom.roomWidth / 2, newRoom.roomWidth / 2 + lastRoom.roomWidth + 20);

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

    private List<Vector2Int> PickDoorsForRoom(Dictionary<string, Vector2Int> corners)
    {
        List<Vector2Int> doors = new List<Vector2Int>();

        // pick a random edge tile from each wall. Use the Room corners dictionary to define a wall
        
        Vector2Int topDoor = new Vector2Int(
            Random.Range(corners["TopLeft"].x + 1, corners["TopRight"].x - 2), // Use 1 for the first value and 2 for the second because the first value already excludes that number (I dont want a door with 2 gridspaces of a corner)
            corners["TopLeft"].y
        );
        Vector2Int leftDoor = new Vector2Int(
            corners["TopLeft"].x,
            Random.Range(corners["TopLeft"].y - 1, corners["BottomLeft"].y + 2)
        );
        Vector2Int bottomDoor = new Vector2Int(
            Random.Range(corners["BottomLeft"].x + 1, corners["BottomRight"].x - 2), // Use 1 for the first value and 2 for the second because the first value already excludes that number (I dont want a door with 2 gridspaces of a corner)
            corners["BottomLeft"].y
        );
        Vector2Int rightDoor = new Vector2Int(
            corners["TopRight"].x,
            Random.Range(corners["TopRight"].y - 1, corners["BottomRight"].y + 2)
        );

        doors.Add(topDoor);
        doors.Add(rightDoor);
        doors.Add(bottomDoor);
        doors.Add(leftDoor);
        
        return doors;
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