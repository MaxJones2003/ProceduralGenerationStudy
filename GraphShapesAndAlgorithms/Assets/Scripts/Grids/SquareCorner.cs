using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SquareCorner
{
    public SquareEdge[] Protrudes { get; set; }
    public SquareFace[] Touches { get; set; }
    public SquareCorner[] Adjacent { get; set; }

    public int Q { get; set; }
    public  int R { get; set; }
    public SquareCorner(int q, int r)
    {
        Q = q;
        R = r;
    }

}
