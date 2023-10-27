using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class PathNode
    {
        public int G;
        public int H;
        public int F { get { return G + H; } }

        public bool isBlocked;
        public PathNode previousNode;

        public Vector2Int position;

        public PathNode(Vector2Int position, bool isBlocked)
        {
            this.position = position;
            this.isBlocked = isBlocked;
        }
    }
}











 #region Old Node
    /* public int gCost, hCost;
    public bool obstacle;
    public Vector3 worldPosition;

    public int GridX, GridY;
    public Node2D parent;


    public Node2D(bool _obstacle, Vector3 _worldPos, int _gridX, int _gridY)
    {
        obstacle = _obstacle;
        worldPosition = _worldPos;
        GridX = _gridX;
        GridY = _gridY;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }

    }
    

    public void SetObstacle(bool isOb)
    {
        obstacle = isOb;
    } */
#endregion
