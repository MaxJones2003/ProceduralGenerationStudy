using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoronoiDiagram))]
public class VoronoiDiagramEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VoronoiDiagram voronoi = (VoronoiDiagram)target;

        if (GUILayout.Button("Generate Voronoi Diagram"))
        {
            voronoi.GenerateVoronoi();
        }

        base.OnInspectorGUI();
    }
}
