using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Geometry;

[CustomEditor(typeof(SquareGridController))]
public class SquareGridEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        SquareGridController grid = (SquareGridController)target;

        if (GUILayout.Button("Generate Grid"))
        {
            grid.GenerateGrid();
        }
        base.OnInspectorGUI();

    }
    
}
