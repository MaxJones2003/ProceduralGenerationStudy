using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HallwayPathfinder
{
    private readonly List<Room> rooms;
    private Vector2Int currentMinBounds;
    private Vector2Int currentMaxBounds;
    private List<HallGridPosition> currentGrid;
    public HallwayPathfinder(List<Room> Rooms)
    {
        rooms = Rooms;
    }

    #region Grid Setup
    public List<HallGridPosition> SetUpGrid(Hallway hallway)
    {
        if (hallway.MultiConnectionHall)
            return SetUpMultiRoomGrid(hallway);

        int minX, maxX, minY, maxY;
        FindMinMaxXYGridPositions(hallway.From, hallway.To, out minX, out maxX, out minY, out maxY);
        minX -= 15;
        minY -= 15;
        maxX += 15;
        maxY += 15;
        currentMinBounds = new Vector2Int(minX, minY);
        currentMaxBounds = new Vector2Int(maxX, maxY);

        // Create a list of HallGridPosition for all positions within these bounds; all positions should be set isFree
        List<Room> roomsInBounds = FindRoomsInBounds(currentMinBounds, currentMaxBounds);

        List<HallGridPosition> hallGrid;
        InitializeHallGridPositions(currentMinBounds, currentMaxBounds, roomsInBounds, out hallGrid);

        return hallGrid;
    }

    public List<HallGridPosition> SetUpMultiRoomGrid(Hallway hallway)
    {
        // Get all of the rooms from the dictionary and add the room to a list
        List<Room> hallwayRooms = new();
        foreach (var item in hallway.MultiConnectionPointList)
        {
            int roomNumber = item.Key;
            Room room = rooms.FirstOrDefault(r => r.roomNumber == roomNumber);
            hallwayRooms.Add(room);
        }
        int minX = 0, maxX = 0, minY = 0, maxY = 0;
        FindMinMaxXYGridPositions(hallway, ref minX, ref maxX, ref minY, ref maxY);
        minX -= 15;
        minY -= 15;
        maxX += 15;
        maxY += 15;
        currentMinBounds = new Vector2Int(minX, minY);
        currentMaxBounds = new Vector2Int(maxX, maxY);

        // Create a list of HallGridPosition for all positions within these bounds; all positions should be set isFree
        List<Room> roomsInBounds = FindRoomsInBounds(currentMinBounds, currentMaxBounds);

        List<HallGridPosition> hallGrid;
        InitializeHallGridPositions(currentMinBounds, currentMaxBounds, roomsInBounds, out hallGrid);

        return hallGrid;
    }

    public void FindMinMaxXYGridPositions(Vector2Int fromRoom, Vector2Int toRoom, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = fromRoom.x; maxX = fromRoom.x; minY = fromRoom.y; maxY = fromRoom.y;
        // Update the minimum and maximum values based on the froomRoom position
        minX = Mathf.Min(minX, fromRoom.x);
        maxX = Mathf.Max(maxX, fromRoom.x);
        minY = Mathf.Min(minY, fromRoom.y);
        maxY = Mathf.Max(maxY, fromRoom.y);
        
        // Update the minimum and maximum values based on the toRoom position
        minX = Mathf.Min(minX, toRoom.x);
        maxX = Mathf.Max(maxX, toRoom.x);
        minY = Mathf.Min(minY, toRoom.y);
        maxY = Mathf.Max(maxY, toRoom.y);
        
    }

    public void FindMinMaxXYGridPositions(Hallway hallway, ref int minX, ref int maxX, ref int minY, ref int maxY)
    {
        foreach(var item in hallway.MultiConnectionPointList)
        {
            minX = Mathf.Min(minX, item.Value.x);
            maxX = Mathf.Max(maxX, item.Value.x);
            minY = Mathf.Min(minY, item.Value.y);
            maxY = Mathf.Max(maxY, item.Value.y);
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
                bool isFree = true;
                if(roomsInBounds.Any(room => room.doors.Contains(new Vector2Int(x, y)))) isFree = true;
                else if(roomsInBounds.Any(room => room.roomEdges.Contains(new Vector2Int(x, y)))) isFree = false;
                hallGrid.Add(new HallGridPosition(new Vector2Int(x, y), isFree, -1));
            }
        }
    }
    #endregion

    #region Pathfinding
    public List<Vector2Int> Search(Hallway hallway, ref List<HallGridPosition> grid)
    {
        // Get Start and end position
        Vector2Int startPos = hallway.From;
        Vector2Int endPos = hallway.To;
        currentGrid = grid;
        // Check if startPos is within the grid bounds
        if (!IsPositionWithinBounds(startPos))
        {
            Debug.LogError("Start position is out of bounds.");
            return new List<Vector2Int>();
        }

        // Mark the start position as visited
        currentGrid.FirstOrDefault(item => item.pos == startPos).Visited = 0;

        
        int xDifference = currentMaxBounds.x - currentMinBounds.x;
        int yDifference = currentMaxBounds.y - currentMinBounds.y;
        int arraySize = Mathf.Abs(xDifference * yDifference);
        int[] testArray = new int[arraySize];
        int steps = 0;

        List<HallGridPosition> positionsWithStep = FindPositionsAtStep(0);

        for (int step = 1; step < arraySize; step++)
        {
            foreach (var item in positionsWithStep)
            {
                if (item.Visited == step - 1)
                {
                    TestFourDirections(item.pos, step);
                }
            }
            steps = step;
            positionsWithStep = FindPositionsAtStep(step);
        }
        grid = currentGrid;
        return SetPath(hallway, steps, out grid);
    }
    private List<HallGridPosition> FindPositionsAtStep(int step)
    {
        List<HallGridPosition> grid = new();
        foreach(var pos in currentGrid)
            if(pos.Visited == step)
                grid.Add(pos);
        return grid;
    }


    private List<Vector2Int> SetPath(Hallway hallway, int step, out List<HallGridPosition> grid)
    {
        grid = currentGrid;
        Vector2Int startPos = hallway.To;
        List<HallGridPosition> tempList = new List<HallGridPosition>();
        List<HallGridPosition> path = new List<HallGridPosition>();
        
        // Check if the end position is out of bounds or not reached by the pathfinder
        if (!IsPositionWithinBounds(startPos))
        {
            Debug.LogError("Desired Path Location Is Out Of Bounds.");
            return new List<Vector2Int>();
        }
        if(currentGrid.FirstOrDefault(item => item.pos == startPos).Visited < 0)
        {
            Debug.LogError($"Door From IsFree {currentGrid.FirstOrDefault(item => item.pos == hallway.From).isFree}. Door To IsFree {currentGrid.FirstOrDefault(item => item.pos == hallway.To).isFree}");
            Debug.LogError($"Hallway room {hallway.fromRoomNumber} had trouble. Is MultiRoom? {hallway.MultiConnectionHall}. Start Position Visited: {currentGrid.FirstOrDefault(item => item.pos == startPos).Visited}");
            return new List<Vector2Int>();
        }

        Vector2Int pos = startPos;
        path.Add(currentGrid.FirstOrDefault(item => item.pos == pos));
        step = currentGrid.FirstOrDefault(item => item.pos == pos).Visited - 1;

        for (int i = step; i > -1; i--)
        {
            Vector2Int dir = Vector2Int.up;
            if (TestDirection(pos, i, dir))
            {
                pos.x = pos.x + dir.x;
                pos.y = pos.y + dir.y;
                tempList.Add(currentGrid.FirstOrDefault(item => item.pos == pos));
            }
            dir = Vector2Int.down;
            if (TestDirection(pos, i, dir))
            {
                pos.x = pos.x + dir.x;
                pos.y = pos.y + dir.y;
                tempList.Add(currentGrid.FirstOrDefault(item => item.pos == pos));
            }

            dir = Vector2Int.left;
            if (TestDirection(pos, i, dir))
            {
                pos.x = pos.x + dir.x;
                pos.y = pos.y + dir.y;
                tempList.Add(currentGrid.FirstOrDefault(item => item.pos == pos));
            }

            dir = Vector2Int.right;
            if (TestDirection(pos, i, dir))
            {
                pos.x = pos.x + dir.x;
                pos.y = pos.y + dir.y;
                tempList.Add(currentGrid.FirstOrDefault(item => item.pos == pos));
            }

            HallGridPosition tempPosition = FindClosest(currentGrid.FirstOrDefault(item => item.pos == pos).pos, tempList);

            // Check if the tempPosition is within bounds before adding it to the path
            if (IsPositionWithinBounds(tempPosition.pos))
            {
                path.Add(tempPosition);
                pos.x = tempPosition.pos.x;
                pos.y = tempPosition.pos.y;
            }
            tempList.Clear();
        }

        List<Vector2Int> pathPositions = new List<Vector2Int>();
        foreach (var item in path)
            pathPositions.Add(item.pos);

        if(hallway.MultiConnectionHall)
            SetMultiHallPath(hallway, ref pathPositions);
        grid = currentGrid;
        return pathPositions;
    }
    private void SetMultiHallPath(Hallway hallway, ref List<Vector2Int> path)
    {
        for(int i = 2; i < hallway.MultiConnectionPointList.Count; i++)
        {
            // Find the start point in the path based on the end position
        }
    }

    private void TestFourDirections(Vector2Int pos, int step)
    {
        if (TestDirection(pos, -1, Vector2Int.up))
            SetVisited(pos + Vector2Int.up, step);

        if (TestDirection(pos, -1, Vector2Int.down))
            SetVisited(pos + Vector2Int.down, step);

        if (TestDirection(pos, -1, Vector2Int.left))
            SetVisited(pos + Vector2Int.left, step);

        if (TestDirection(pos, -1, Vector2Int.right))
            SetVisited(pos + Vector2Int.right, step);
    }
    private bool TestDirection(Vector2Int pos, int step, Vector2Int direction)
    {
        // Check if the new position is within the bounds of the grid
        if (IsPositionWithinBounds(pos+direction))
        {
            // Check if the Visited value at the new position is equal to HallGridPosition.Visited
            // and if the grid space is free
            Vector2Int newPos = pos+direction;
            var gridItem = currentGrid.FirstOrDefault(item => item.pos == newPos);
            //if(gridItem.Visited == step && gridItem.isFree) Debug.Log("Free and -1 Step at: " + (pos+direction));
            if(gridItem.Visited == step && gridItem.isFree)
                return true;
        }
        return false;
    }

    private void SetVisited(Vector2Int pos, int step)
    {
        currentGrid.FirstOrDefault(item => item.pos == pos).Visited = step;
    }

    private HallGridPosition FindClosest(Vector2Int targetPos, List<HallGridPosition> list)
    {
        int xDifference = currentMaxBounds.x - currentMinBounds.x;
        int yDifference = currentMaxBounds.y - currentMinBounds.y;
        float currentDistance = Mathf.Abs(xDifference * yDifference);

        int indexNumber = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if ((targetPos - list[i].pos).magnitude < currentDistance)
            {
                currentDistance = (targetPos - list[i].pos).magnitude;
                indexNumber = i;
            }
        }
        return list[indexNumber];
    }

    private bool IsPositionWithinBounds(Vector2Int pos)
    {
        return pos.x >= currentMinBounds.x && pos.x <= currentMaxBounds.x &&
               pos.y >= currentMinBounds.y && pos.y <= currentMaxBounds.y;
    }
    #endregion
}

public class HallGridPosition
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
}


