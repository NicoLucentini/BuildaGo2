using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TalentManager))]
public class TalentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        // Reference to the actual target script
        TalentManager manager = (TalentManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Update Nodes"))
        {
            manager.UpdateNodes();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Clear Nodes"))
        {
            manager.ClearNodes();
        }
    }
}