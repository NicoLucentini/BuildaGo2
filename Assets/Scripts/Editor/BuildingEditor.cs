using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Building))]
public class BuildingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        // Reference to the actual target script
        Building manager = (Building)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Invoke OnStartConstruction Event"))
        {
            manager.TriggerStartConstructionEvent();
        }
    }
}