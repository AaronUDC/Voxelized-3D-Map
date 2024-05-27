using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapSaveData
{
	public float voxelSize;

	public int chunkResolution, mapResolution;

	public List<VoxelChunkData> chunks;

    [System.Serializable]	
	public struct VoxelChunkData{
		public string chunkName;
		public int[] voxelIDs;
		public BlockData[] blocks;
	}
	[System.Serializable]
	public struct BlockData{
		public int index;
		public int id;
		public int rotation;

	}
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}
public interface ISaveableMap
{
    void PopulateSaveData(MapSaveData saveData);
    void LoadFromSaveData(MapSaveData saveData);
}