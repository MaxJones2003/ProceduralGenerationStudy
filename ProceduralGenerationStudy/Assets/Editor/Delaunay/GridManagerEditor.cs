using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
        {
            GridManager gridManager = (GridManager)target;

            if(GUILayout.Button("Generate"))
            {
                gridManager.Generate();
            }
            if(GUILayout.Button("Generate Random"))
            {
                Seed.Instance.RandomizeSeed();
                gridManager.Generate();
            }
            base.OnInspectorGUI();
        }
}
