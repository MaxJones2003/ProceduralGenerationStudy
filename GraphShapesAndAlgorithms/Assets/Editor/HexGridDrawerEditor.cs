using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexGridDrawer))]
public class HexGridDrawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGridDrawer myScript = (HexGridDrawer)target;
        if (GUILayout.Button("Generate Grid"))
        {
            myScript.GenerateGrid();
        }
        if (GUILayout.Button("Relax Grid"))
        {
            myScript.RelaxGrid();
        }
    }
}
