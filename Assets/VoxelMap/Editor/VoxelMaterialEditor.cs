using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelMaterial))]
public class VoxelMaterialEditor : Editor{
	public override void OnInspectorGUI()
	{	
		VoxelMaterial voxelMaterial = (VoxelMaterial)target;

		if(GUILayout.Button("Build from atlas")){
			voxelMaterial.LoadPartAtlas();

			EditorUtility.SetDirty(target);
			Undo.RecordObject(target, "Build voxel material from atlas");
		}

		if(GUILayout.Button("Clear Data")){
			voxelMaterial.ClearData();
			
			EditorUtility.SetDirty(target);
			Undo.RecordObject(target, "Clear voxel material data");
		}

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Data");
		
		DrawDefaultInspector();




	}
}
