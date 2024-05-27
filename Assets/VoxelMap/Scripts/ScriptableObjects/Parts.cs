using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PartDB", menuName = "ScriptableObjects/VoxelMap/PartDB", order = 2)]
public class PartDB : ScriptableObject
{
	[Serializable]
    public class VoxelMaterialsDictionary : SerializableDictionary<int,VoxelMaterial> {}
	
	[Serializable]
    public class BlocksDictionary : SerializableDictionary<int,Block> {}

	public VoxelMaterialsDictionary voxelMaterials;
	public BlocksDictionary blocks;
}
