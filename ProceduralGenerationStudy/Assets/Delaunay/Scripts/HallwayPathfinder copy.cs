using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

public class HallwayPathfinderCopy
{
    private readonly List<Room> rooms;

    public HallwayPathfinderCopy(List<Room> Rooms)
    {
        rooms = Rooms;
    }

    #region Grid Setup
    public HallGridPosition[,] SetUpGrid(Hallway hallway)
    {
        if (hallway.MultiConnectionHall)
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

        // Create a HallGridPosition array for all positions within these bounds; all positions should be set isFree
        List<Room> roomsInBounds = FindRoomsInBounds(minGridPos, maxGridPos);

        HallGridPosition[,] hallGrid;
        InitializeHallGridPositions(minGridPos, maxGridPos, roomsInBounds, out hallGrid);

        return hallGrid;
    }

    public HallGridPosition[,] SetUpMultiRoomGrid(Hallway hallway)
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

        // Create a HallGridPosition array for all positions within these bounds; all positions should be set isFree
        List<Room> roomsInBounds = FindRoomsInBounds(minGridPos, maxGridPos);

        HallGridPosition[,] hallGrid;
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
        foreach (var room in rooms)
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

    public void InitializeHallGridPositions(Vector2Int minBounds, Vector2Int maxBounds, List<Room> roomsInBounds, out HallGridPosition[,] hallGrid)
    {
        int width = maxBounds.x - minBounds.x + 1;
        int height = maxBounds.y - minBounds.y + 1;
        hallGrid = new HallGridPosition[width, height];

        for (int x = minBounds.x; x <= maxBounds.x; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                bool isFree = !roomsInBounds.Any(room => room.roomGridPositions.Contains(new Vector2Int(x, y)));
                hallGrid[x - minBounds.x, y - minBounds.y] = new HallGridPosition(new Vector2Int(x, y), isFree, -1);
            }
        }
    }
    #endregion

    #region Pathfinding
    public List<Vector2Int> Search(Hallway hallway)
    {
        // Get Start and end position
        Vector2Int startPos = hallway.From;
        Vector2Int endPos = hallway.To;
        Debug.Log(hallway.HallwayGridPositionsCopy.GetLength(0) + " " + hallway.HallwayGridPositionsCopy.GetLength(1));
        HallGridPosition[,] grid = hallway.HallwayGridPositionsCopy;
        Debug.Log(startPos);
        grid[startPos.x, startPos.y].Visited = 0;
        if(hallway.MultiConnectionHall) Debug.Log("hi");
        
        int[] testArray = new int[grid.GetLength(0) * grid.GetLength(1)];

        for(int step = 1; step < grid.GetLength(0) * grid.GetLength(1); step++)
        {
            foreach(var item in grid)
            {
                if(item.Visited == step - 1)
                    TestFourDirections(item.pos, step, grid);
            }
        }

        return SetPath(hallway, grid);
    }

    private List<Vector2Int> SetPath(Hallway hallway, HallGridPosition[,] grid)
    {
        int step;
        Vector2Int pos = hallway.To;
        List<HallGridPosition> tempList = new();
        List<HallGridPosition> path = new();
        if(pos.x <= 0 || pos.x > grid.GetLength(0) || pos.y <= 0 || pos.y > grid.GetLength(1) || grid[pos.x, pos.y].Visited < 0)
        {
            // End position is out of the grid or was never reached by the pathfinder
            Debug.LogError("Desired Path Location Can't Be Reached.");
            return new List<Vector2Int>(); 
        } 
        
        path.Add(grid[pos.x, pos.y]);
        step = grid[pos.x, pos.y].Visited - 1;

        int x, y;
        for (int i = step; step > -1; step--)
        {
            Vector2Int dir = Vector2Int.up;
            if(TestDirection(pos, step, dir, grid))
            {
                x = pos.x + dir.x;
                y = pos.y + dir.y;
                tempList.Add(grid[x, y]);
            }
            dir = Vector2Int.down;
            if(TestDirection(pos, step, dir, grid))
            {
                x = pos.x + dir.x;
                y = pos.y + dir.y;
                tempList.Add(grid[x, y]);
            }

            dir = Vector2Int.left;
            if(TestDirection(pos, step, dir, grid))
            {
                x = pos.x + dir.x;
                y = pos.y + dir.y;
                tempList.Add(grid[x, y]);
            }

            dir = Vector2Int.right;
            if(TestDirection(pos, step, dir, grid))
            {
                x = pos.x + dir.x;
                y = pos.y + dir.y;
                tempList.Add(grid[x, y]);
            }
            HallGridPosition tempPosition = FindClosest(grid[pos.x, pos.y].pos, tempList, grid);
            path.Add(tempPosition);
            x = tempPosition.pos.x;
            y = tempPosition.pos.y;
            tempList.Clear();
        }
        List<Vector2Int> pathPositions = new();
        foreach(var item in path)
            pathPositions.Add(item.pos);

        return pathPositions;
    }
    private void TestFourDirections(Vector2Int pos, int step, HallGridPosition[,] grid)
    {
        if(TestDirection(pos, -1, Vector2Int.up, grid))
            SetVisited(pos+Vector2Int.up, step, grid);

        if(TestDirection(pos, -1, Vector2Int.down, grid))
            SetVisited(pos+Vector2Int.down, step, grid);

        if(TestDirection(pos, -1, Vector2Int.left, grid))
            SetVisited(pos+Vector2Int.left, step, grid);

        if(TestDirection(pos, -1, Vector2Int.right, grid))
            SetVisited(pos+Vector2Int.right, step, grid);
    }
    private bool TestDirection(Vector2Int pos, int step, Vector2Int direction, HallGridPosition[,] grid)
    {
        int x = pos.x + direction.x;
        int y = pos.y + direction.y;

        // Check if the new position is within the bounds of the grid
        if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
        {
            // Check if the Visited value at the new position is equal to HallGridPosition.Visited
            // and if the grid space is free
            return grid[x, y].Visited == step && grid[x, y].isFree;
        }

        return false;
    }
    private void SetVisited(Vector2Int pos, int step, HallGridPosition[,] grid)
    {
        int x = pos.x; int y = pos.y;
        if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
        {
            // Check if the position is within the bounds of the grid
            grid[x, y].Visited = step;
        }
    }
    private HallGridPosition FindClosest(Vector2Int targetPos, List<HallGridPosition> list, HallGridPosition[,] grid)
    {
        float currentDistance = grid.GetLength(0) * grid.GetLength(1);
        int indexNumber = 0;
        for(int i = 0; i < list.Count; i++)
        {
            if((targetPos - list[i].pos).magnitude < currentDistance)
            {
                currentDistance = (targetPos - list[i].pos).magnitude;
                indexNumber = i;
            }
        }
        return list[indexNumber];
    }
    #endregion
}

/* public class HallGridPosition
{
    public Vector2Int pos;
    public bool isFree;
    public int Visited = -1;

    public HallGridPosition(Vector2Int Pos, bool IsFree, int visited)
    {
        pos = Pos;
        isFree = IsFree;
        Visited = visited;
    }
} */