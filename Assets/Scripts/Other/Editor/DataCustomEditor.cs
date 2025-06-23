using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(Data<>), true)]
public class DataCustomEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the 'value' property inside the generic class
        SerializedProperty valueProp = property.FindPropertyRelative("val");

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        // 👇 This will draw the foldout + nested fields, like default Unity behavior
        EditorGUI.PropertyField(position, property, label, true);


        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();

            object parentObject = fieldInfo.GetValue(property.serializedObject.targetObject);
            var type = parentObject.GetType();
            var callbackField = type.GetField("onValueChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var field = type.GetField("value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (callbackField != null)
            {
                var callback = callbackField.GetValue(parentObject) as Delegate;
                
                //callback?.DynamicInvoke(GetSerializedValue(valueProp));
                callback?.DynamicInvoke(field.GetValue(parentObject) );


                //Debug.Log($"Value in {property.displayName} changed to: {GetSerializedValue(valueProp)} and invoked callback");
            }
        }

        EditorGUI.EndProperty();
    }

    private object GetSerializedValue(SerializedProperty prop)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: return prop.intValue;
            case SerializedPropertyType.Float: return prop.floatValue;
            case SerializedPropertyType.Boolean: return prop.boolValue;
            case SerializedPropertyType.String: return prop.stringValue;
            case SerializedPropertyType.Vector3: return prop.vector3Value;
            case SerializedPropertyType.Vector2Int: return prop.vector2IntValue;
            case SerializedPropertyType.Vector2: return prop.vector2Value;
            // You can extend this with more types
            default: return "Unsupported type";
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true); // include children = true
    }
}