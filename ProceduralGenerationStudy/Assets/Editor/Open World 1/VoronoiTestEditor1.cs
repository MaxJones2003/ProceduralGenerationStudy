using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoronoiTest2))]
public class VoronoiTest2Editor : Editor
{
    public override void OnInspectorGUI()
    {
        VoronoiTest2 voronoiTest = (VoronoiTest2)target;

        if(DrawDefaultInspector ())
        {

        }

        if(GUILayout.Button("Generate A"))
        {
            voronoiTest.VoronoiSetup();
        }
        if(GUILayout.Button("Erase"))
        {
            voronoiTest.Erase();
        }

    }
}
