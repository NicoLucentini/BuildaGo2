using UnityEngine;
using UnityEditor;

using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ShowInInspectorEditor : Editor
{
    /*
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var targetObj = target;
        var type = targetObj.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            bool shouldShow = field.GetCustomAttribute<ShowInInspectorAttribute>() != null;
            if (shouldShow)
            {
                object value = field.GetValue(targetObj);
                EditorGUILayout.LabelField(field.Name, value?.ToString() ?? "null");
            }
        }
    }
    */
}
