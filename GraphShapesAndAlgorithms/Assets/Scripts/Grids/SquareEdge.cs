using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SquareEdge
{
    public SquareFace[] Joins { get; set; }
    public SquareEdge[] Continues { get; set; }
    public SquareCorner[] Endpoints { get; set; }

    public SquareEdge(SquareCorner corner1, SquareCorner corner2)
    {
        Endpoints = new SquareCorner[2]
        {
            corner1,
            corner2
        };
    }
}
