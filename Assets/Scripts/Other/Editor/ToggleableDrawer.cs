#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ToggleableAttribute))]
public class ToggleableDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ToggleableAttribute toggleAttr = (ToggleableAttribute)attribute;

        // Calculate rects
        Rect toggleRect = new Rect(position.x, position.y, 18, position.height);
        Rect fieldRect = new Rect(position.x + 20, position.y, position.width - 20, position.height);

        // Get saved toggle state
        string key = property.propertyPath + "_toggle";
        bool toggle = EditorPrefs.GetBool(key, toggleAttr.defaultState);

        // Draw toggle
        bool newToggle = EditorGUI.Toggle(toggleRect, toggle);

        // Save toggle state
        if (newToggle != toggle)
            EditorPrefs.SetBool(key, newToggle);

        // Disable field if toggle is false
        EditorGUI.BeginDisabledGroup(!newToggle);

        EditorGUI.PropertyField(fieldRect, property, label, true);

        EditorGUI.EndDisabledGroup();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
#endif
