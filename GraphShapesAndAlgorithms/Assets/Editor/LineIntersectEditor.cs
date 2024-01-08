using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Geometry;

[CustomEditor(typeof(LineIntersect))]
public class LineIntersectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LineIntersect grid = (LineIntersect)target;

        if (GUILayout.Button("Order Points"))
        {
            grid.Order();
        }
        base.OnInspectorGUI();

    }
}
