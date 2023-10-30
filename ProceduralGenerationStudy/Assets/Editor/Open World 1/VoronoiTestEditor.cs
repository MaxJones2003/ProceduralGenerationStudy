using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VoronoiTest))]
public class VoronoiTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VoronoiTest voronoiTest = (VoronoiTest)target;

        if(DrawDefaultInspector ())
        {

        }

        if(GUILayout.Button("Generate"))
        {
            voronoiTest.generatePress = true;
            voronoiTest.GenerateShape();
        }

    }
}
