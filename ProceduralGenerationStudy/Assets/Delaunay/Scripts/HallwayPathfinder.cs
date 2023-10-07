using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HallwayPathfinder
{
    private readonly List<Room> rooms;
    
    public HallwayPathfinder(List<Room> Rooms)
    {
        rooms = Rooms;
    }
    #region Grid Setup
    public List<HallGridPosition> SetUpGrid(Hallway hallway)
    {
        if(hallway.MultiConnectionHall)
            return SetUpMultiRoomGrid(hallway);
        
        Room fromRoom = rooms.FirstOrDefault(r => r.roomNumber == hallway.fromRoomNumber);
        Room toRoom = rooms.FirstOrDefault(r => r.roomNumber == hallway.toRoomNumber); 
        

        if (fromRoom == null || toRoom == null)
        {
            Debug.LogError("One of the rooms wasn't found");
            return null;
        } 

        int minX, maxX, minY, maxY;
        FindMinMaxXYGridPositions(fromRoom, toRoom, out minX, out maxX, out minY, out maxY);
        minX -= 5;
        minY -= 5;
        maxX += 5;
        maxY += 5;
        Vector2Int minGridPos = new Vector2Int(minX, minY);
        Vector2Int maxGridPos = new Vector2Int(maxX, maxY);

        // Create a HallGridPosition for all positions within these bounds all positions should be set isFree
        List<Room> roomsInBounds = FindRoomsInBounds(minGridPos, maxGridPos);

        List<HallGridPosition> hallGrid;
        InitializeHallGridPositions(minGridPos, maxGridPos, roomsInBounds, out hallGrid);

        return hallGrid;
    }
    public List<HallGridPosition> SetUpMultiRoomGrid(Hallway hallway)
    {
        // Get all of the rooms from the dictionary add the room to a list
        List<Room> hallwayRooms = new();
        foreach (var item in hallway.MultiConnectionPointList)
        {
            int roomNumber = item.Key;
            Room room = rooms.FirstOrDefault(r => r.roomNumber == roomNumber);
            hallwayRooms.Add(room);
        }
        int minX, maxX, minY, maxY;
        FindMinMaxXYGridPositions(hallwayRooms, out minX, out maxX, out minY, out maxY);
        minX -= 5;
        minY -= 5;
        maxX += 5;
        maxY += 5;
        Vector2Int minGridPos = new Vector2Int(minX, minY);
        Vector2Int maxGridPos = new Vector2Int(maxX, maxY);

        // Create a HallGridPosition for all positions within these bounds all positions should be set isFree
        List<Room> roomsInBounds = FindRoomsInBounds(minGridPos, maxGridPos);

        List<HallGridPosition> hallGrid;
        InitializeHallGridPositions(minGridPos, maxGridPos, roomsInBounds, out hallGrid);

        return hallGrid;
    }
    public void FindMinMaxXYGridPositions(Room fromRoom, Room toRoom, out int minX, out int maxX, out int minY, out int maxY)
    {
        // Initialize the variables to their extreme values
        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        maxY = int.MinValue;

        // Iterate through the edges of the 'fromRoom'
        foreach (Vector2Int edge in fromRoom.roomEdges)
        {
            // Update the minimum and maximum values as needed
            minX = Mathf.Min(minX, edge.x);
            maxX = Mathf.Max(maxX, edge.x);
            minY = Mathf.Min(minY, edge.y);
            maxY = Mathf.Max(maxY, edge.y);
        }

        // Iterate through the edges of the 'toRoom'
        foreach (Vector2Int edge in toRoom.roomEdges)
        {
            // Update the minimum and maximum values as needed
            minX = Mathf.Min(minX, edge.x);
            maxX = Mathf.Max(maxX, edge.x);
            minY = Mathf.Min(minY, edge.y);
            maxY = Mathf.Max(maxY, edge.y);
        }
    }
    public void FindMinMaxXYGridPositions(List<Room> rooms, out int minX, out int maxX, out int minY, out int maxY)
    {
        // Initialize the variables to their extreme values
        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        maxY = int.MinValue;

        // Iterate through the edges each room
        foreach(var room in rooms)
            foreach (Vector2Int edge in room.roomEdges)
            {
                // Update the minimum and maximum values as needed
                minX = Mathf.Min(minX, edge.x);
                maxX = Mathf.Max(maxX, edge.x);
                minY = Mathf.Min(minY, edge.y);
                maxY = Mathf.Max(maxY, edge.y);
            }
    }
    public List<Room> FindRoomsInBounds(Vector2Int minBounds, Vector2Int maxBounds)
    {
        List<Room> roomsInBounds = new List<Room>();

        foreach (Room room in rooms)
        {
            // Check if 'roomCenterPosition' is within the specified bounds
            if (room.roomCenterPosition.x >= minBounds.x && room.roomCenterPosition.x <= maxBounds.x &&
                room.roomCenterPosition.y >= minBounds.y && room.roomCenterPosition.y <= maxBounds.y)
            {
                roomsInBounds.Add(room);
            }
        }

        return roomsInBounds;
    }
    public void InitializeHallGridPositions(Vector2Int minBounds, Vector2Int maxBounds, List<Room> roomsInBounds, out List<HallGridPosition> hallGrid)
    {
        hallGrid = new List<HallGridPosition>();
        for (int x = minBounds.x; x <= maxBounds.x; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                bool isFree = !roomsInBounds.Any(room => room.roomGridPositions.Contains(new Vector2Int(x, y)));
                hallGrid.Add(new HallGridPosition(new Vector2Int(x, y), isFree));
            }
        }
    }
    #endregion

    #region Pathfinding
    public void Search(Hallway hallway)
    {
        Debug.Log("Stinky!!! You have dis many grid spaces: " + hallway.HallwayGridPositions.Count);
    }
    #endregion
}


public class HallGridPosition
{
    public Vector2Int pos;
    public bool isFree;
    public bool isVisited;

    public HallGridPosition(Vector2Int Pos, bool IsFree)
    {
        pos = Pos;
        isFree = IsFree;
    }
}
