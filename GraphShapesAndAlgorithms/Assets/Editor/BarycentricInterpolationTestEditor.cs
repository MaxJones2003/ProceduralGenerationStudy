using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BarycentricInterpolationTest))]
public class BarycentricInterpolationTestEditor : Editor
{
    /* public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BarycentricInterpolationTest myScript = (BarycentricInterpolationTest)target;
        if(GUILayout.Button("Order Corners Clockwise"))
        {
            myScript.OrderCorners();
        }
    } */
}