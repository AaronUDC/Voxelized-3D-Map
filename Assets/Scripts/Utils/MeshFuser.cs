using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshFuser
{
    public struct PartData{
        public int meshindex;
        public Matrix4x4 localToWorldMatrix;
        //public Material[] materials;

    }

    public struct MeshData{
        public Mesh mesh;
        public Material[] materials;

    }

    public MeshData[] meshes;

    

    public MeshFuser(MeshData[] meshes){
        this.meshes = meshes;
    }


    public MeshData FuseMeshes(PartData[] partdatas){
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        //Setup array of arrays of combineInstances.
        //Probably can be optimized with multithreading.

        foreach(PartData part in partdatas){
            MeshData meshData = this.meshes[part.meshindex];
            for(int s = 0; s < meshData.mesh.subMeshCount; s++){
                
                int materialArrayIndex = SearchForMaterial(materials, meshData.materials[s].name);

                if(materialArrayIndex == -1){
                    materials.Add(meshData.materials[s]);
                    materialArrayIndex = materials.Count - 1;

                }

                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance{
                    transform = part.localToWorldMatrix,
                    subMeshIndex = s,
                    mesh = meshData.mesh
                };
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);

            }
        }

        //Combine meshes by material
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for( int m = 0; m < materials.Count; m++){
            CombineInstance[] combineInstanceArray = 
                (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            
            meshes[m] = new Mesh();
            meshes[m].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshes[m].CombineMeshes(combineInstanceArray,true,true);

            combineInstances[m] = new CombineInstance
            {
                mesh = meshes[m],
                subMeshIndex = 0
            };
        }

        //Combining into one mesh
        Mesh outMesh = new Mesh();
        outMesh.CombineMeshes(combineInstances, false,false);

        //Setting the combined mesh data
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];

        MeshData outMeshData = new MeshData
        {
            materials = materialsArray,
            mesh = outMesh
        };

        return outMeshData;

    }

    int SearchForMaterial(ArrayList materialList, string searchName){

        for(int i = 0; i < materialList.Count; i++){
            if(((Material)materialList[i]).name == searchName)
                return i;
        }

        return -1;
    }
}
