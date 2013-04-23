using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(CSharpPropertyAttribute))]
public class CSharpPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var propertyAttribute = attribute as CSharpPropertyAttribute;
		var csharpProperty = property.serializedObject.FindProperty(propertyAttribute.PropertyName);

		//EditorGUI.Slider(position, property, 0, 180, label);
		if (csharpProperty != null)
			DrawDefaultProperty(position, csharpProperty, label);
		else
			EditorGUI.TextArea(position, "Property not found");
	}

	public void DrawDefaultProperty(Rect position, SerializedProperty property, GUIContent label, Boolean includeChildren = true)
	{
		const BindingFlags AllBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		var PropertyDrawers = typeof(PropertyDrawer).GetField("s_PropertyDrawers", AllBindingFlags).GetValue(null) as Dictionary<string, PropertyDrawer>;

		foreach (var Pair in PropertyDrawers.Where(entry => entry.Value == this))
		{
			PropertyDrawers[Pair.Key] = null;
			EditorGUI.PropertyField(position, property, label, true);
			PropertyDrawers[Pair.Key] = this;

			return;
		}

		EditorGUI.PropertyField(position, property, label, true);
	}
}
