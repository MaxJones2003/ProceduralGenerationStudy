using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KDTREETEST))]
public class KdTreeTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        KDTREETEST myScript = (KDTREETEST)target;
        if (GUILayout.Button("Test"))
        {
            myScript.Go(0, myScript.stage);
        }
    }
}
