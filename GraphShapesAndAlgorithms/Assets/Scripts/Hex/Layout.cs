using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct Layout
{
    public readonly Orientation orientation;
    public readonly Point size;
    public readonly Point origin;
    public Layout(Orientation orientation, Point size, Point origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }

    public static Layout flat = new Layout(Orientation.layout_flat, new Point(1.0f, 1.0f), new Point(0.0f, 0.0f));
    public static Layout pointy = new Layout(Orientation.layout_pointy, new Point(1.0f, 1.0f), new Point(0.0f, 0.0f));
}
