using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PiecePlacementSystem))]
public class PiecePlacementSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var placementSystem = (PiecePlacementSystem)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Print FEN Notation"))
            {
                placementSystem.SaveFenToSO();
            }
        }
    }
}
