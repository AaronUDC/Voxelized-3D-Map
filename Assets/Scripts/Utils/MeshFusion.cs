using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshFusion : MonoBehaviour
{

    public List<Mesh> meshes;

    public Vector3Int chunkShape;

    private MeshFilter meshFilter;

    private MeshRenderer meshRenderer;


    public bool addCollider = false;

    public Mesh currentMesh;



    // Start is called before the first frame update
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        //Transform[] transform = gameObject.GetComponentsInChildren<Transform>();
        //Debug.Log(transform[1].gameObject.name+ " " + transform[1].localToWorldMatrix);
        FuseMeshes(gameObject.GetComponentsInChildren<MeshFilter>());


    }
    void Start()
    {
        
        /*MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length-1];

        int i = 0,j = 0;
        while (j < meshFilters.Length)
        {
            if(meshFilters[j].sharedMesh !=null){
                combine[i].mesh = meshFilters[j].sharedMesh;
                combine[i].transform = meshFilters[j].transform.localToWorldMatrix;
                meshFilters[j].gameObject.SetActive(false);
                i++;
            }
            j++;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine,false,true);
        mesh.name = "Chunk";
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;

        transform.gameObject.SetActive(true);*/
    }


    //From https://pastebin.com/aUwvqFKJ
    void FuseMeshes(MeshFilter[] meshFilters){

        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

        foreach (MeshFilter meshFilter in meshFilters){
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if(!meshRenderer || !meshFilter.sharedMesh || meshRenderer.materials.Length != meshFilter.sharedMesh.subMeshCount) 
                continue;

            for(int s = 0; s < meshFilter.sharedMesh.subMeshCount;s++){
                int materialArrayIndex = ContainsMaterial(materials, meshRenderer.sharedMaterials[s].name);

                if (materialArrayIndex == -1){
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    materialArrayIndex = materials.Count -1;
                }

                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance
                {
                    transform = meshRenderer.transform.localToWorldMatrix,
                    subMeshIndex = s,
                    mesh = meshFilter.sharedMesh
                };
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        MeshFilter meshFilterCombine = gameObject.GetComponent<MeshFilter>();
        MeshRenderer meshRendererCombine = gameObject.GetComponent<MeshRenderer>();

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

        // Combine into one
        meshFilterCombine.sharedMesh = new Mesh();
        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

        FixPos();

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        if (addCollider)
        {
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        }

        // Delete the child GameObjects to reduce draw calls and time spent on the render thread.
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            print("CombineMeshes.cs: About to delete child: " + transform.GetChild(i).name);
            Destroy(transform.GetChild(i).gameObject);
        }

    }

    int ContainsMaterial(ArrayList searchList, String name){

        for(int i = 0; i< searchList.Count; i++){

            if(((Material)searchList[i]).name == name){
                return i;
            }
        }

        return -1;
    }

    void FixPos()
    {
        // Set the position of the combined mesh to the position of the original objects.
        transform.localScale = new Vector3(1f, 1f, 1f);
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
    }

}
