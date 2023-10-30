using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SquareFace
{
    public SquareFace[] Neighbors { get; set; }
    public SquareEdge[] Borders { get; set; }
    public SquareCorner[] Corners { get; set; }

    public SquareFace(SquareCorner corner1, SquareCorner corner2, SquareCorner corner3, SquareCorner corner4)
    {
        Corners = new SquareCorner[4]
        {
            corner1,
            corner2,
            corner3,
            corner4
        };
    }
}
