using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSaveLoad : MonoBehaviour
{

	public VoxelMap map;

	public string saveFileName = "Map";

	void OnGUI(){
		float width = 150f, height = 200f;
		GUILayout.BeginArea(new Rect( Screen.width-width-10, 10, width, height));

		saveFileName = GUILayout.TextField(saveFileName);

		if(GUILayout.Button("Save map"))
			SaveMapAs(saveFileName);


		if(GUILayout.Button("Load map"))
			LoadMapAs(saveFileName);

		GUILayout.EndArea();
	}
	
	void SaveMapAs(string name){
		MapSaveDataManager.SaveJsonData(new ISaveableMap[]{map}, name);
	}
    void LoadMapAs(string name){
		MapSaveDataManager.LoadJsonData(new ISaveableMap[]{map}, name);
	}
}
