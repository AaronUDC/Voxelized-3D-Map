using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "VoxelMaterial", menuName = "ScriptableObjects/VoxelMap/VoxelMaterial", order = 1)]
public class VoxelMaterial : ScriptableObject
{
/*  private static readonly string[] partNames = new string[]
    {
        "0.01",
        "1.01", "1.02",
        "2.01", "2.02", "2.03",
        "3.01", "3.02", "3.03", "3.04",
        "4.01", "4.02", "4.03", "4.04", "4.05", "4.06", "4.07", "4.08",
        "5.01", "5.02", "5.03", "5.04", "5.05", "5.06", "5.07", "5.08",
        "6.01", "6.02", "6.03", "6.04", "6.05", "6.06", "6.07", "6.08",
        "7.01","7.02",
        "8.01"
        };
 */
    //MARK: Options
	[SerializeField]
    private bool useAtlas;
	[SerializeField]
    private GameObject partAtlas;

    //MARK: Data

    [Serializable]
    public class PartData{
        public Mesh mesh;
        public int[] materialIndexes;

        public PartData(Mesh mesh, int[] materialIndex){
            this.mesh = mesh;
            this.materialIndexes = materialIndex;
        }
    }

    [Serializable]
    public class PartsDictionary : SerializableDictionary<string,PartData> {}

    public PartsDictionary basicParts;

    public Material[] materials;


    //MARK: Load part Atlas

	public void LoadPartAtlas(){
		if(useAtlas) LoadPartAtlas(partAtlas);
	}
    public void LoadPartAtlas(GameObject atlas){

        ArrayList materialsArray = new ArrayList();
        MeshFilter[] parts = atlas.transform.GetComponentsInChildren<MeshFilter>();

        basicParts = new PartsDictionary();        

        foreach( MeshFilter part in parts){

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
