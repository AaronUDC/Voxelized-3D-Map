using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "VoxelMaterial", menuName = "ScriptableObjects/VoxelMap/VoxelMaterial", order = 1)]
public class VoxelMaterial : ScriptableObject
{

    //MARK: Options
	
	[SerializeField]
    public GameObject tileset;

    //MARK: Data

    [Serializable]
    public class PartData{
        public Mesh mesh;
        public int[] materialIndexes;

        public PartData(Mesh mesh, int[] materialIndexes){
            this.mesh = mesh;
            this.materialIndexes = materialIndexes;
        }
    }

    [Serializable]
    public class PartsDictionary : SerializableDictionary<string,PartData> {}
	
    public PartsDictionary basicParts;

    public Material[] materials;


    //MARK: Load part Atlas

	public void LoadTileset(){
		LoadTileset(tileset);
	}
    public void LoadTileset(GameObject atlas){

        ArrayList materialsArray = new ArrayList();
        MeshFilter[] parts = atlas.transform.GetComponentsInChildren<MeshFilter>();

        basicParts = new PartsDictionary();        

        foreach( MeshFilter part in parts){

			if(ChunkBuilder.PART_NAMES.Contains(part.name)){

				//Materials
				MeshRenderer meshRenderer = part.GetComponent<MeshRenderer>();
				int[] materialsIndexes = new int[meshRenderer.sharedMaterials.Length];

				for(int i = 0; i < materialsIndexes.Length; i++){

					int materialArrayIndex = SearchForMaterial(materialsArray, meshRenderer.sharedMaterials[i].name);

					if(materialArrayIndex == -1){
						materialsArray.Add(meshRenderer.sharedMaterials[i]);
						materialArrayIndex = materialsArray.Count - 1;

					}
					materialsIndexes[i] = materialArrayIndex;
				}
				PartData partData = new PartData(part.sharedMesh,materialsIndexes);
				basicParts.Add(part.transform.name,partData);
			}else{
				Debug.LogWarning("Part: " + part.name + " not recognized.");
			}
		}

		this.materials = materialsArray.ToArray(typeof(Material)) as Material[];

		
    }

	public void ClearData(){
		basicParts.Clear();
		materials = new Material[0];
	}

    //MARK: Search for material
    int SearchForMaterial(ArrayList materialList, string searchName){

        for(int i = 0; i < materialList.Count; i++){
            if(((Material)materialList[i]).name == searchName)
                return i;
        }

        return -1;
    }

  
}
