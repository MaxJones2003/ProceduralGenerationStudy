using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallwayPathfinder
{
    public List<Vector2Int> CreateHallwayPath(Hallway hallway)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        //if(hallway.MultiConnectionHall)
            path = ConnectDoors(hallway.From, hallway.To);
        return path;
    }
    private List<Vector2Int> ConnectDoors(Vector2Int from, Vector2Int to)
    {
        // Start at the from door move towards the to door
        Vector2Int currentPosition = from;
        Vector2Int previousPosition = from;
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int direction = from - to;
        int count = 0;

        while(currentPosition != to)
        {
            if(count < 5)
            {
                (previousPosition, currentPosition) = MoveStraight(previousPosition, currentPosition, to);
                path.Add(currentPosition);
            }
            else
            {
                currentPosition = to;
            }
            count++;
        }
        return path;
    }
    private (Vector2Int, Vector2Int) MoveStraight(Vector2Int previousPos, Vector2Int pos, Vector2Int destination)
    {
        // Check the previous movement. Compare the current position with the previous one
        Vector2Int difference = pos - previousPos;
        // Since we are changing the value of pos, to a new value, the current value (pos) becomes the previous value (previousPos)
        previousPos = pos;
        if(difference == Vector2Int.zero)
        {
            // This means that the previous position and the current are the same, in turn, that means this is the first time the position is changing so we must pick a dirction to go
            Vector2Int direction = destination - pos;
            if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Move along the x axis, because we must go further in that direction
                // We must find whether the value is positive or negative
                int posOrNegOne = direction.x / Mathf.Abs(direction.x); // If direction is negative, this will equal negative one, if its positive, it will equal one
                pos.x += posOrNegOne;
                return (previousPos, pos);
            }
            else
            {
                // Move along the y axis, because we must go further in that direction
                // We must find whether the value is positive or negative
                int posOrNegOne = direction.y / Mathf.Abs(direction.y); // If direction is negative, this will equal negative one, if its positive, it will equal one
                pos.y += posOrNegOne;
                return (previousPos, pos);
            }
        }

        pos += difference;

        return (previousPos, pos);
    }
    
}
