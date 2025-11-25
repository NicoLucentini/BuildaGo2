using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Toggleable<>))]
public class GenericToggleableDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
        SerializedProperty valueProp = property.FindPropertyRelative("value");
        return  enabledProp.boolValue == true ? EditorGUI.GetPropertyHeight(valueProp, true) + EditorGUI.GetPropertyHeight(enabledProp, true) :
            EditorGUI.GetPropertyHeight(enabledProp, true)
            ;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
        SerializedProperty valueProp = property.FindPropertyRelative("value");

        float toggleHeight = EditorGUI.GetPropertyHeight(enabledProp, true);
        float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);

        // Correct rects
        var toggleRect = new Rect(position.x, position.y, position.width, toggleHeight);
        var valueRect = new Rect(position.x, position.y + toggleHeight + 2, position.width, valueHeight);

        // Draw toggle
        enabledProp.boolValue = EditorGUI.ToggleLeft(toggleRect, property.displayName, enabledProp.boolValue);

        // Draw value only when enabled
        if (enabledProp.boolValue)
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none, true);

        EditorGUI.EndProperty();
    }
}
