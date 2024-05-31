using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelMaterial))]
[CanEditMultipleObjects]
public class VoxelMaterialEditor : Editor{

	SerializedProperty tilesetProperty;
	SerializedProperty partsProperty;
	SerializedProperty materialsProperty;

	//bool showData = false;
	void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        tilesetProperty = serializedObject.FindProperty("tileset");
        partsProperty = serializedObject.FindProperty("basicParts");
        materialsProperty = serializedObject.FindProperty("materials");
    }

	
	public override void OnInspectorGUI()
	{	
		VoxelMaterial voxelMaterial = (VoxelMaterial)target;


		//EditorGUILayout.ObjectField("Script", target, GetType(), false);	

		

		if(GUILayout.Button("Clear Data")){
			voxelMaterial.ClearData();
			EditorUtility.SetDirty(target);
			Undo.RecordObject(target, "Clear voxel material data");

			
			serializedObject.Update();
		}

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(tilesetProperty, new GUIContent("Tileset"));

		if(GUILayout.Button("Build from tileset")){
			voxelMaterial.ClearData();
			voxelMaterial.LoadTileset();

			EditorUtility.SetDirty(target);
			Undo.RecordObject(target, "Build voxel material from tileset");
			
			serializedObject.Update();
			
		}

		EditorGUILayout.Space();
	
		EditorGUILayout.LabelField("Data");
		EditorGUILayout.PropertyField(partsProperty,true);
		EditorGUILayout.PropertyField(materialsProperty);


		//DrawDefaultInspector();
		serializedObject.ApplyModifiedProperties();
		
		

	}
}
